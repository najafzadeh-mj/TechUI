namespace TechUI.SampleAdmin.Models;

public sealed class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? Description { get; set; }
}
