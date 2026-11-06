using System.Text.Json.Serialization;

namespace TechUI.SampleAdmin.Models;

public sealed class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string City { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public decimal Credit { get; set; }

    public DateTime RegisteredOn { get; set; }

    public string? Tags { get; set; }

    public string? Notes { get; set; }

    [JsonIgnore]
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
