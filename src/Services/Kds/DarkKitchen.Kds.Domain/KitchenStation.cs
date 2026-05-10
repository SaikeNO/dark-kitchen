namespace DarkKitchen.Kds.Domain;

public sealed class KitchenStation
{
    private KitchenStation()
    {
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string DisplayColor { get; private set; } = "#2f7d57";
    public bool IsActive { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static KitchenStation Create(
        Guid id,
        string code,
        string name,
        string displayColor,
        bool isActive,
        DateTimeOffset now)
    {
        var station = new KitchenStation { Id = id };
        station.ApplyCatalogChange(code, name, displayColor, isActive, now);
        return station;
    }

    public void ApplyCatalogChange(
        string code,
        string name,
        string displayColor,
        bool isActive,
        DateTimeOffset now)
    {
        Code = code;
        Name = name;
        DisplayColor = displayColor;
        IsActive = isActive;
        UpdatedAt = now;
    }
}
