using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public CheckoutController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var cartItems = await _context.CartItems
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var viewModel = new CheckoutViewModel
        {
            TotalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSession(CheckoutViewModel model)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var user = await _context.Users.FindAsync(userId);
        var cartItems = await _context.CartItems
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        var totalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity);

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            ShippingAddress = model.ShippingAddress
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                BookId = item.BookId,
                Quantity = item.Quantity,
                UnitPrice = item.Book.Price
            });

            item.Book.StockQuantity -= item.Quantity;
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        var lineItems = cartItems.Select(item => new Stripe.Checkout.SessionLineItemOptions
        {
            PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
            {
                Currency = "usd",
                ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Book.Title,
                },
                UnitAmount = (long)(item.Book.Price * 100),
            },
            Quantity = item.Quantity,
        }).ToList();

        var options = new Stripe.Checkout.SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}&order_id={order.Id}",
            CancelUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Cancel?order_id={order.Id}",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() }
            }
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);

        _context.Payments.Add(new Payment
        {
            OrderId = order.Id,
            StripeSessionId = session.Id,
            Amount = totalAmount,
            Status = "pending"
        });

        order.Status = OrderStatus.Paid;
        await _context.SaveChangesAsync();

        return Redirect(session.Url);
    }

    public async Task<IActionResult> Success(string session_id, int order_id)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == order_id);

        if (order == null) return NotFound();

        try
        {
            var service = new Stripe.Checkout.SessionService();
            var session = await service.GetAsync(session_id);

            if (session.PaymentStatus == "paid" && order.Payment != null)
            {
                order.Payment.StripePaymentIntentId = session.PaymentIntentId;
                order.Payment.Status = "completed";
                order.Status = OrderStatus.Processing;
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
        }

        return View(order);
    }

    public async Task<IActionResult> Cancel(int order_id)
    {
        var order = await _context.Orders.FindAsync(order_id);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
        }
        return View();
    }
}
