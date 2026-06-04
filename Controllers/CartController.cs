using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await GetCartItems();
        var viewModel = new CartViewModel { Items = items };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int bookId, int quantity = 1)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null || book.StockQuantity < quantity)
            return NotFound();

        var userId = GetUserId();
        var sessionId = GetSessionId();

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c =>
                c.BookId == bookId &&
                (userId != null ? c.UserId == userId : c.SessionId == sessionId));

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                BookId = bookId,
                UserId = userId,
                SessionId = userId == null ? sessionId : null,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, int quantity)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item == null) return NotFound();

        if (quantity <= 0)
            return RedirectToAction(nameof(Remove), new { id });

        item.Quantity = quantity;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await _context.CartItems.FindAsync(id);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();
        var items = await _context.CartItems
            .Where(c => userId != null ? c.UserId == userId : c.SessionId == sessionId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<int> GetCartCount()
    {
        var items = await GetCartItems();
        return items.Sum(i => i.Quantity);
    }

    private async Task<List<CartItem>> GetCartItems()
    {
        var userId = GetUserId();
        var sessionId = GetSessionId();

        return await _context.CartItems
            .Include(c => c.Book)
            .Where(c => userId != null ? c.UserId == userId : c.SessionId == sessionId)
            .ToListAsync();
    }

    private string? GetUserId()
    {
        if (User.Identity?.IsAuthenticated == true)
            return _context.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
        return null;
    }

    private string GetSessionId()
    {
        if (HttpContext.Session.GetString("CartSessionId") == null)
            HttpContext.Session.SetString("CartSessionId", Guid.NewGuid().ToString());
        return HttpContext.Session.GetString("CartSessionId")!;
    }
}
