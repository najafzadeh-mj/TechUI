namespace TechUI.SampleAdmin.ViewModels;

public sealed class ReportsViewModel
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int TotalOrders { get; set; }

    public int DeliveredCount { get; set; }

    public int CanceledCount { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal AverageOrder { get; set; }

    public List<StatusSlice> Statuses { get; set; } = new();
}
