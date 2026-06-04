using BookBazaar.Models;

namespace BookBazaar.ViewModels;

public class BookListViewModel
{
    public List<Book> Books { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
