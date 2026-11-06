namespace TechUI.SampleAdmin.Models;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Canceled = 4
}

public static class OrderStatusExtensions
{
    public static string ToPersian(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "در انتظار پرداخت",
        OrderStatus.Paid => "پرداخت شده",
        OrderStatus.Shipped => "ارسال شده",
        OrderStatus.Delivered => "تحویل شده",
        OrderStatus.Canceled => "لغو شده",
        _ => "نامشخص"
    };

    public static string ToBadgeClass(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "bg-warning text-dark",
        OrderStatus.Paid => "bg-primary",
        OrderStatus.Shipped => "bg-info text-dark",
        OrderStatus.Delivered => "bg-success",
        OrderStatus.Canceled => "bg-danger",
        _ => "bg-secondary"
    };
}
