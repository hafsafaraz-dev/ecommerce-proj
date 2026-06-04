namespace BookBazaar.Models;

public class Wishlist
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
