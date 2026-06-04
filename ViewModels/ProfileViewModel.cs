using System.ComponentModel.DataAnnotations;
using BookBazaar.Models;

namespace BookBazaar.ViewModels;

public class ProfileViewModel
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public List<Order> RecentOrders { get; set; } = new();
    public List<Review> RecentReviews { get; set; } = new();
}

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
