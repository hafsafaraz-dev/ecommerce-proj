using BookBazaar.Models;

namespace BookBazaar.ViewModels;

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Book.Price * i.Quantity);
    public int ItemCount => Items.Sum(i => i.Quantity);
}
