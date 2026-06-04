using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Controllers;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search, string? sort, int page = 1)
    {
        var query = _context.Books
            .Include(b => b.Category)
            .Include(b => b.Reviews)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(term) || b.Author.ToLower().Contains(term) || b.Description.ToLower().Contains(term));
        }

        sort = sort?.ToLower();
        query = sort switch
        {
            "price_asc" => query.OrderBy(b => b.Price),
            "price_desc" => query.OrderByDescending(b => b.Price),
            "rating" => query.OrderByDescending(b => b.Reviews.Where(r => !r.IsHidden).Average(r => (double?)r.Rating) ?? 0),
            "newest" => query.OrderByDescending(b => b.CreatedAt),
            _ => query.OrderByDescending(b => b.CreatedAt)
        };

        int pageSize = 12;
        int totalBooks = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

        var books = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await _context.Categories.ToListAsync();

        var viewModel = new BookListViewModel
        {
            Books = books,
            Categories = categories,
            SelectedCategoryId = categoryId,
            SearchTerm = search,
            SortBy = sort,
            Page = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound();

        var userName = User.Identity?.Name;
        var userId = userName != null ? _context.Users.Where(u => u.UserName == userName).Select(u => u.Id).FirstOrDefault() : null;
        bool hasReviewed = userId != null && await _context.Reviews.AnyAsync(r => r.BookId == id && r.UserId == userId);
        bool isInWishlist = false;

        if (userId != null)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.UserId == userId);
            isInWishlist = wishlist?.Items.Any(i => i.BookId == id) ?? false;
        }

        var visibleReviews = book.Reviews.Where(r => !r.IsHidden).ToList();
        var starDist = new int[5];
        foreach (var r in visibleReviews)
            if (r.Rating >= 1 && r.Rating <= 5)
                starDist[r.Rating - 1]++;

        var viewModel = new BookDetailViewModel
        {
            Book = book,
            Reviews = visibleReviews.OrderByDescending(r => r.CreatedAt).ToList(),
            HasUserReviewed = hasReviewed,
            IsInWishlist = isInWishlist,
            StarDistribution1 = starDist[0],
            StarDistribution2 = starDist[1],
            StarDistribution3 = starDist[2],
            StarDistribution4 = starDist[3],
            StarDistribution5 = starDist[4]
        };

        return View(viewModel);
    }
}
