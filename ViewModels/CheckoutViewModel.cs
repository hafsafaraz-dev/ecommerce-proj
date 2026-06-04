using System.ComponentModel.DataAnnotations;

namespace BookBazaar.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [StringLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    public string? StripePublishableKey { get; set; }
    public string? ClientSecret { get; set; }
    public int? OrderId { get; set; }
}
