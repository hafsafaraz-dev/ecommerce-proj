using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookBazaar.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Reviews)
            .Where(b => b.IsFeatured)
            .Take(8)
            .ToListAsync();

        var recent = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.Reviews)
            .OrderByDescending(b => b.CreatedAt)
            .Take(8)
            .ToListAsync();

        var categories = await _context.Categories.ToListAsync();

        var viewModel = new BookListViewModel
        {
            Books = recent,
            Categories = categories
        };

        ViewBag.FeaturedBooks = featured;
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
