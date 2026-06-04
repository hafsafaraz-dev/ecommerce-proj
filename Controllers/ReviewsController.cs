using BookBazaar.Data;
using BookBazaar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int bookId, int rating, string title, string comment)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        if (userId == null) return Challenge();

        var existing = await _context.Reviews.AnyAsync(r => r.BookId == bookId && r.UserId == userId);
        if (existing)
        {
            TempData["Error"] = "You have already reviewed this book.";
            return RedirectToAction("Details", "Books", new { id = bookId });
        }

        _context.Reviews.Add(new Review
        {
            BookId = bookId,
            UserId = userId,
            Rating = rating,
            Title = title,
            Comment = comment
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Review submitted successfully.";
        return RedirectToAction("Details", "Books", new { id = bookId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int rating, string title, string comment)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        var review = await _context.Reviews.FindAsync(id);

        if (review == null || review.UserId != userId)
            return NotFound();

        review.Rating = rating;
        review.Title = title;
        review.Comment = comment;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Review updated.";
        return RedirectToAction("Details", "Books", new { id = review.BookId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _context.Users.Where(u => u.UserName == User.Identity!.Name).Select(u => u.Id).FirstOrDefault();
        var review = await _context.Reviews.FindAsync(id);

        if (review == null || review.UserId != userId)
            return NotFound();

        int bookId = review.BookId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Review deleted.";
        return RedirectToAction("Details", "Books", new { id = bookId });
    }
}
