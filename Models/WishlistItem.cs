namespace BookBazaar.Models;

public class WishlistItem
{
    public int Id { get; set; }

    public int WishlistId { get; set; }

    public Wishlist Wishlist { get; set; } = null!;

    public int BookId { get; set; }

    public Book Book { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
