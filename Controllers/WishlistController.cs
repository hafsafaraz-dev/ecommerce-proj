using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly ApplicationDbContext _context;

    public WishlistController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
                .ThenInclude(wi => wi.Book)
                    .ThenInclude(b => b.Category)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        return View(wishlist?.Items ?? new List<WishlistItem>());
    }

    [HttpPost]
    public async Task<IActionResult> Add(int bookId)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var wishlist = await _context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            wishlist = new Wishlist { UserId = userId };
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
        }

        if (!wishlist.Items.Any(i => i.BookId == bookId))
        {
            wishlist.Items.Add(new WishlistItem { BookId = bookId, WishlistId = wishlist.Id });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await _context.WishlistItems.FindAsync(id);
        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MoveToCart(int id)
    {
        var item = await _context.WishlistItems
            .Include(wi => wi.Book)
            .FirstOrDefaultAsync(wi => wi.Id == id);

        if (item == null) return NotFound();

        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();

        var existingCart = await _context.CartItems
            .FirstOrDefaultAsync(c => c.BookId == item.BookId && c.UserId == userId);

        if (existingCart != null)
            existingCart.Quantity++;
        else
            _context.CartItems.Add(new CartItem { BookId = item.BookId, UserId = userId, Quantity = 1 });

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
