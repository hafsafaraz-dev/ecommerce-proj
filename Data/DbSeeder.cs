using BookBazaar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookBazaar.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Customer"))
            await roleManager.CreateAsync(new IdentityRole("Customer"));

        var adminEmail = "admin@bookbazaar.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Admin",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        var userEmail = "user@bookbazaar.com";
        var customer = await userManager.FindByEmailAsync(userEmail);
        if (customer == null)
        {
            customer = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "Test User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(customer, "User@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(customer, "Customer");
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Programming", Slug = "programming" },
                new() { Name = "Fiction", Slug = "fiction" },
                new() { Name = "Science", Slug = "science" },
                new() { Name = "History", Slug = "history" },
                new() { Name = "Business", Slug = "business" }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        if (await context.Books.AnyAsync(b => b.CoverImage == null))
        {
            context.OrderItems.RemoveRange(context.OrderItems);
            context.Orders.RemoveRange(context.Orders);
            context.Reviews.RemoveRange(context.Reviews);
            context.Books.RemoveRange(context.Books);
            await context.SaveChangesAsync();
        }

        if (!await context.Books.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var sampleBooks = new List<Book>
            {
                new() { Title = "Clean Code", Author = "Robert C. Martin", Description = "A handbook of agile software craftsmanship.", Price = 39.99m, StockQuantity = 20, CategoryId = categories[0].Id, IsFeatured = true, CoverImage = "https://picsum.photos/seed/cleancode/300/400" },
                new() { Title = "The Pragmatic Programmer", Author = "David Thomas", Description = "Timeless tips for software developers.", Price = 44.99m, StockQuantity = 15, CategoryId = categories[0].Id, IsFeatured = false, CoverImage = "https://picsum.photos/seed/pragmatic/300/400" },
                new() { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Description = "The story of Jay Gatsby's pursuit of the American Dream.", Price = 12.99m, StockQuantity = 30, CategoryId = categories[1].Id, IsFeatured = true, CoverImage = "https://picsum.photos/seed/gatsby/300/400" },
                new() { Title = "1984", Author = "George Orwell", Description = "A dystopian novel set in a totalitarian society.", Price = 14.99m, StockQuantity = 25, CategoryId = categories[1].Id, IsFeatured = false, CoverImage = "https://picsum.photos/seed/nineteen84/300/400" },
                new() { Title = "A Brief History of Time", Author = "Stephen Hawking", Description = "From the Big Bang to black holes.", Price = 18.99m, StockQuantity = 12, CategoryId = categories[2].Id, IsFeatured = true, CoverImage = "https://picsum.photos/seed/hawking/300/400" },
                new() { Title = "Sapiens", Author = "Yuval Noah Harari", Description = "A brief history of humankind.", Price = 24.99m, StockQuantity = 22, CategoryId = categories[2].Id, IsFeatured = false, CoverImage = "https://picsum.photos/seed/sapiens/300/400" },
                new() { Title = "Guns, Germs, and Steel", Author = "Jared Diamond", Description = "The fates of human societies.", Price = 16.99m, StockQuantity = 18, CategoryId = categories[3].Id, IsFeatured = false, CoverImage = "https://picsum.photos/seed/gunsgerms/300/400" },
                new() { Title = "The Lean Startup", Author = "Eric Ries", Description = "How today's entrepreneurs use continuous innovation.", Price = 21.99m, StockQuantity = 20, CategoryId = categories[4].Id, IsFeatured = true, CoverImage = "https://picsum.photos/seed/leanstartup/300/400" }
            };
            context.Books.AddRange(sampleBooks);
            await context.SaveChangesAsync();

            var reviews = new List<Review>
            {
                new() { BookId = sampleBooks[0].Id, UserId = (await userManager.FindByEmailAsync(adminEmail))!.Id, Rating = 5, Title = "Must read", Comment = "Every developer should read this.", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new() { BookId = sampleBooks[1].Id, UserId = (await userManager.FindByEmailAsync(userEmail))!.Id, Rating = 4, Title = "Great insights", Comment = "Very practical advice.", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { BookId = sampleBooks[2].Id, UserId = (await userManager.FindByEmailAsync(userEmail))!.Id, Rating = 5, Title = "Classic", Comment = "Beautifully written story.", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { BookId = sampleBooks[4].Id, UserId = (await userManager.FindByEmailAsync(adminEmail))!.Id, Rating = 4, Title = "Fascinating", Comment = "Hawking makes complex ideas accessible.", CreatedAt = DateTime.UtcNow.AddDays(-7) }
            };
            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }
    }
}
