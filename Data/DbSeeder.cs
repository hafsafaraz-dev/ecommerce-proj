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

        if (!await context.Books.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var rand = new Random();
            var books = new List<Book>();
            var programmingBooks = new[]
            {
                ("Clean Code", "Robert C. Martin", "A handbook of agile software craftsmanship."),
                ("The Pragmatic Programmer", "David Thomas", "Timeless tips for software developers."),
                ("Design Patterns", "Gang of Four", "Elements of reusable object-oriented software."),
                ("Introduction to Algorithms", "Thomas H. Cormen", "Comprehensive algorithms reference."),
                ("C# in Depth", "Jon Skeet", "Deep dive into modern C# features."),
                ("ASP.NET Core in Action", "Andrew Lock", "Building web apps with ASP.NET Core."),
                ("Code Complete", "Steve McConnell", "Practical handbook of software construction."),
                ("Refactoring", "Martin Fowler", "Improving the design of existing code."),
                ("The Mythical Man-Month", "Fred Brooks", "Essays on software engineering."),
                ("Structure and Interpretation of Computer Programs", "Harold Abelson", "Classic CS textbook.")
            };
            var fictionBooks = new[]
            {
                ("The Great Gatsby", "F. Scott Fitzgerald", "The story of Jay Gatsby's pursuit of the American Dream."),
                ("To Kill a Mockingbird", "Harper Lee", "A novel about racial injustice in the Deep South."),
                ("1984", "George Orwell", "A dystopian novel set in a totalitarian society."),
                ("Pride and Prejudice", "Jane Austen", "A romantic novel of manners in Georgian England."),
                ("The Catcher in the Rye", "J.D. Salinger", "A story of teenage alienation."),
                ("Brave New World", "Aldous Huxley", "A dystopian vision of a genetically engineered future."),
                ("The Hobbit", "J.R.R. Tolkien", "Bilbo Baggins' unexpected adventure."),
                ("Dune", "Frank Herbert", "Epic science fiction set on the desert planet Arrakis."),
                ("The Alchemist", "Paulo Coelho", "A shepherd boy's journey to find treasure."),
                ("One Hundred Years of Solitude", "Gabriel García Márquez", "Multi-generational story of the Buendía family.")
            };
            var scienceBooks = new[]
            {
                ("A Brief History of Time", "Stephen Hawking", "From the Big Bang to black holes."),
                ("The Selfish Gene", "Richard Dawkins", "A gene-centered view of evolution."),
                ("Cosmos", "Carl Sagan", "A personal voyage through the universe."),
                ("The Origin of Species", "Charles Darwin", "The foundational work of evolutionary biology."),
                ("Sapiens", "Yuval Noah Harari", "A brief history of humankind."),
                ("The Gene", "Siddhartha Mukherjee", "An intimate history of genetics."),
                ("Astrophysics for People in a Hurry", "Neil deGrasse Tyson", "Quick guide to the cosmos."),
                ("The Elegant Universe", "Brian Greene", "Superstrings, hidden dimensions, and quantum mechanics."),
                ("Gödel, Escher, Bach", "Douglas Hofstadter", "An eternal golden braid of patterns."),
                ("The Double Helix", "James D. Watson", "The discovery of DNA structure.")
            };
            var historyBooks = new[]
            {
                ("Guns, Germs, and Steel", "Jared Diamond", "The fates of human societies."),
                ("The Art of War", "Sun Tzu", "Ancient Chinese military treatise."),
                ("The Rise and Fall of the Third Reich", "William L. Shirer", "History of Nazi Germany."),
                ("The Silk Roads", "Peter Frankopan", "A new history of the world."),
                ("SPQR", "Mary Beard", "A history of ancient Rome."),
                ("The Histories", "Herodotus", "The first great history book in Western civilization."),
                ("1776", "David McCullough", "The story of America's founding year."),
                ("The Diary of a Young Girl", "Anne Frank", "A girl's experience during the Holocaust."),
                ("A People's History of the United States", "Howard Zinn", "American history from below."),
                ("The Guns of August", "Barbara W. Tuchman", "The outbreak of World War I.")
            };
            var businessBooks = new[]
            {
                ("The Lean Startup", "Eric Ries", "How today's entrepreneurs use continuous innovation."),
                ("Good to Great", "Jim Collins", "Why some companies make the leap and others don't."),
                ("The 7 Habits of Highly Effective People", "Stephen R. Covey", "Powerful lessons in personal change."),
                ("Thinking, Fast and Slow", "Daniel Kahneman", "The two systems that drive our decisions."),
                ("Zero to One", "Peter Thiel", "Notes on startups, or how to build the future."),
                ("The Intelligent Investor", "Benjamin Graham", "The definitive book on value investing."),
                ("How to Win Friends and Influence People", "Dale Carnegie", "Timeless principles for success."),
                ("Rich Dad Poor Dad", "Robert Kiyosaki", "What the rich teach their kids about money."),
                ("Start with Why", "Simon Sinek", "How great leaders inspire everyone to take action."),
                ("The E-Myth Revisited", "Michael E. Gerber", "Why most small businesses don't work.")
            };

            var allBookSets = new[]
            {
                (programmingBooks, categories[0]),
                (fictionBooks, categories[1]),
                (scienceBooks, categories[2]),
                (historyBooks, categories[3]),
                (businessBooks, categories[4])
            };

            foreach (var (bookSet, category) in allBookSets)
            {
                foreach (var (title, author, description) in bookSet)
                {
                    books.Add(new Book
                    {
                        Title = title,
                        Author = author,
                        Description = description,
                        Price = Math.Round((decimal)(rand.NextDouble() * 35 + 9.99), 2),
                        StockQuantity = rand.Next(5, 50),
                        IsFeatured = rand.Next(0, 5) == 0,
                        CategoryId = category.Id,
                        CoverImage = null,
                        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 365)),
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }

        if (!await context.Reviews.AnyAsync())
        {
            var books = await context.Books.ToListAsync();
            var users = await context.Users.ToListAsync();
            var rand = new Random();
            var reviews = new List<Review>();
            var reviewTitles = new[] { "Great book!", "Highly recommended", "Good read", "Average", "Not my favorite", "Excellent!", "Must read", "Informative", "Well written", "Could be better" };

            for (int i = 0; i < 20 && i < books.Count * users.Count; i++)
            {
                var book = books[rand.Next(books.Count)];
                var user = users[rand.Next(users.Count)];
                if (reviews.Any(r => r.BookId == book.Id && r.UserId == user.Id))
                    continue;
                reviews.Add(new Review
                {
                    BookId = book.Id,
                    UserId = user.Id,
                    Rating = rand.Next(1, 6),
                    Title = reviewTitles[rand.Next(reviewTitles.Length)],
                    Comment = "This is a sample review comment for the book.",
                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 60))
                });
            }
            context.Reviews.AddRange(reviews);
            await context.SaveChangesAsync();
        }

        if (!await context.Orders.AnyAsync())
        {
            var users = await context.Users.ToListAsync();
            var books = await context.Books.ToListAsync();
            var rand = new Random();

            for (int i = 0; i < 10; i++)
            {
                var user = users[rand.Next(users.Count)];
                var orderItems = new List<OrderItem>();
                decimal total = 0;
                int itemCount = rand.Next(1, 5);

                for (int j = 0; j < itemCount; j++)
                {
                    var book = books[rand.Next(books.Count)];
                    var qty = rand.Next(1, 3);
                    var price = book.Price;
                    orderItems.Add(new OrderItem
                    {
                        BookId = book.Id,
                        Quantity = qty,
                        UnitPrice = price
                    });
                    total += price * qty;
                }

                var order = new Order
                {
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{i + 1:D4}",
                    UserId = user.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-rand.Next(1, 30)),
                    TotalAmount = total,
                    Status = (OrderStatus)rand.Next(0, 6),
                    ShippingAddress = "123 Sample Street, Test City",
                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 30))
                };
                context.Orders.Add(order);
                await context.SaveChangesAsync();

                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    context.OrderItems.Add(item);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
