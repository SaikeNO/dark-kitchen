namespace DarkKitchen.Catalog.Domain;

public sealed class ProductStationRoute
{
    private ProductStationRoute()
    {
    }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid StationId { get; private set; }
    public Station? Station { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static ProductStationRoute Create(Guid productId, Guid stationId, DateTimeOffset now)
    {
        return new ProductStationRoute
        {
            ProductId = productId,
            StationId = stationId,
            UpdatedAt = now
        };
    }

    public void ChangeStation(Guid stationId, DateTimeOffset now)
    {
        StationId = stationId;
        UpdatedAt = now;
    }
}
