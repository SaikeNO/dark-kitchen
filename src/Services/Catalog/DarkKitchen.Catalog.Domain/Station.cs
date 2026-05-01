namespace DarkKitchen.Catalog.Domain;

public sealed class Station
{
    private Station()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string DisplayColor { get; private set; } = "#3f7f5f";
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<ProductStationRoute> Routes { get; private set; } = [];

    public static Station Create(
        string code,
        string name,
        string displayColor,
        bool isActive,
        DateTimeOffset now,
        Guid? id = null)
    {
        return new Station
        {
            Id = id ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            DisplayColor = displayColor,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string code, string name, string displayColor, bool isActive, DateTimeOffset now)
    {
        Code = code;
        Name = name;
        DisplayColor = displayColor;
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
