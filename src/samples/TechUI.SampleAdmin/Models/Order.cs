namespace TechUI.SampleAdmin.Models;

public sealed class Order
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal Total { get; set; }

    public string? Note { get; set; }
}
