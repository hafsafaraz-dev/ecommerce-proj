using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

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
            TotalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity),
            StripePublishableKey = _configuration["Stripe:PublishableKey"] ?? ""
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var cartItems = await _context.CartItems
            .Include(c => c.Book)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return RedirectToAction("Index", "Cart");

        if (string.IsNullOrWhiteSpace(model.ShippingAddress))
        {
            ModelState.AddModelError("", "Shipping address is required.");
            return Json(new { success = false, error = "Shipping address is required." });
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        var totalAmount = cartItems.Sum(c => c.Book.Price * c.Quantity);
        var totalCents = (long)(totalAmount * 100);

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

        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = totalCents,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() }
            }
        });

        _context.Payments.Add(new Payment
        {
            OrderId = order.Id,
            StripeSessionId = paymentIntent.Id,
            Amount = totalAmount,
            Status = "pending"
        });
        await _context.SaveChangesAsync();

        return Json(new { success = true, clientSecret = paymentIntent.ClientSecret, orderId = order.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(int orderId, string paymentIntentId)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();

        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(paymentIntentId);

        if (intent.Status == "succeeded" && order.Payment != null)
        {
            order.Payment.StripePaymentIntentId = paymentIntentId;
            order.Payment.Status = "completed";
            order.Status = OrderStatus.Processing;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Success), new { orderId });
        }

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
        TempData["Error"] = "Payment was not successful.";
        return RedirectToAction("Index", "Cart");
    }

    public async Task<IActionResult> Success(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Cancel(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
        }
        return View();
    }
}
