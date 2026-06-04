using BookBazaar.Data;
using BookBazaar.Models;
using BookBazaar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var viewModel = new AdminDashboardViewModel
        {
            TotalRevenue = totalRevenue,
            TotalBooks = await _context.Books.CountAsync(),
            TotalOrders = await _context.Orders.CountAsync(),
            TotalCustomers = await _context.Users.CountAsync(),
            TotalReviews = await _context.Reviews.CountAsync(),
            PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing),
            DeliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered),
            MonthlySales = await GetMonthlySales()
        };

        return View(viewModel);
    }

    private async Task<List<MonthlySales>> GetMonthlySales()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var grouped = await _context.Orders
            .Where(o => o.OrderDate >= sixMonthsAgo && o.Status == OrderStatus.Delivered)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return grouped.Select(g => new MonthlySales
        {
            Month = $"{g.Year}-{g.Month:D2}",
            Revenue = g.Revenue,
            Orders = g.Orders
        }).ToList();
    }
}
