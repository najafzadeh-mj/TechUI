namespace TechUI.SampleAdmin.ViewModels;

public sealed class MapViewModel
{
    public List<CityMapPoint> Cities { get; set; } = new();

    public int CityCount => Cities.Count;

    public int CustomerCount => Cities.Sum(item => item.Customers);

    public int ActiveCustomerCount => Cities.Sum(item => item.ActiveCustomers);

    public int OrderCount => Cities.Sum(item => item.Orders);

    public decimal Revenue => Cities.Sum(item => item.Revenue);
}

public sealed record CityMapPoint(
    string City,
    double Lat,
    double Lng,
    int Customers,
    int ActiveCustomers,
    decimal Credit,
    int Orders,
    decimal Revenue);
