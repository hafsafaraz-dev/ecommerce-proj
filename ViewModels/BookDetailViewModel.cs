using BookBazaar.Models;

namespace BookBazaar.ViewModels;

public class BookDetailViewModel
{
    public Book Book { get; set; } = null!;
    public List<Review> Reviews { get; set; } = new();
    public bool HasUserReviewed { get; set; }
    public bool IsInWishlist { get; set; }
    public int StarDistribution1 { get; set; }
    public int StarDistribution2 { get; set; }
    public int StarDistribution3 { get; set; }
    public int StarDistribution4 { get; set; }
    public int StarDistribution5 { get; set; }
}
