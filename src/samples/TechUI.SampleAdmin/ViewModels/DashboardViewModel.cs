using TechUI.SampleAdmin.Models;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class DashboardViewModel
{
    public int CustomersCount { get; set; }

    public int ActiveCustomersCount { get; set; }

    public int OrdersCount { get; set; }

    public int ProductsCount { get; set; }

    public int LowStockCount { get; set; }

    public decimal Revenue { get; set; }

    public decimal TodayRevenue { get; set; }

    public decimal AverageOrder { get; set; }

    public List<MonthlyPoint> Monthly { get; set; } = new();

    public List<StatusSlice> Statuses { get; set; } = new();

    public List<CitySlice> TopCities { get; set; } = new();

    public List<Order> RecentOrders { get; set; } = new();

    public List<Product> LowStockProducts { get; set; } = new();
}

public sealed record MonthlyPoint(string Label, decimal Amount, int Orders);

public sealed record StatusSlice(
    string Label,
    string BadgeClass,
    int Count,
    int Percent);

public sealed record CitySlice(string City, int Orders, decimal Revenue);
