using BookBazaar.Data;
using BookBazaar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 15;
        var query = _context.Books.Include(b => b.Category);
        var total = await query.CountAsync();
        var books = await query.OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Page = page;
        return View(books);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book, IFormFile? CoverImageFile)
    {
        if (CoverImageFile != null && CoverImageFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverImageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await CoverImageFile.CopyToAsync(stream);
            }
            book.CoverImage = "/images/books/" + fileName;
        }
        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Book created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book, IFormFile? CoverImageFile)
    {
        var existing = await _context.Books.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.ISBN = book.ISBN;
        existing.Description = book.Description;
        existing.Price = book.Price;
        existing.StockQuantity = book.StockQuantity;
        existing.CategoryId = book.CategoryId;
        existing.IsFeatured = book.IsFeatured;
        existing.UpdatedAt = DateTime.UtcNow;

        if (CoverImageFile != null && CoverImageFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CoverImageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await CoverImageFile.CopyToAsync(stream);
            }
            existing.CoverImage = "/images/books/" + fileName;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Book updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
        TempData["Success"] = "Book deleted.";
        return RedirectToAction(nameof(Index));
    }
}
