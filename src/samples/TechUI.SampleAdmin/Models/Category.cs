using System.Text.Json.Serialization;

namespace TechUI.SampleAdmin.Models;

public sealed class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    [JsonIgnore]
    public Category? Parent { get; set; }

    [JsonIgnore]
    public ICollection<Category> Children { get; set; } = new List<Category>();

    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
