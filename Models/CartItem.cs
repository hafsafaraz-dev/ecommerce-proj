using System.ComponentModel.DataAnnotations;

namespace BookBazaar.Models;

public class CartItem
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    public int BookId { get; set; }

    public Book Book { get; set; } = null!;

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
