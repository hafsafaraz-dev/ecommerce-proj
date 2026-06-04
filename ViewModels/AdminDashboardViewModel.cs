namespace BookBazaar.ViewModels;

public class AdminDashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalBooks { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalReviews { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public List<MonthlySales> MonthlySales { get; set; } = new();
}

public class MonthlySales
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}
