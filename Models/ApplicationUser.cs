using Microsoft.AspNetCore.Identity;

namespace BookBazaar.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool IsSuspended { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Wishlist? Wishlist { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
