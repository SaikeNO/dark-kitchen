namespace DarkKitchen.Kds.Domain;

public sealed class ProductStationRouteSnapshot
{
    private ProductStationRouteSnapshot()
    {
    }

    public Guid BrandId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid StationId { get; private set; }
    public string StationCode { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static ProductStationRouteSnapshot Create(
        Guid brandId,
        Guid productId,
        Guid stationId,
        string stationCode,
        DateTimeOffset now)
    {
        return new ProductStationRouteSnapshot
        {
            BrandId = brandId,
            ProductId = productId,
            StationId = stationId,
            StationCode = stationCode,
            UpdatedAt = now
        };
    }

    public void ChangeStation(Guid stationId, string stationCode, DateTimeOffset now)
    {
        StationId = stationId;
        StationCode = stationCode;
        UpdatedAt = now;
    }
}
