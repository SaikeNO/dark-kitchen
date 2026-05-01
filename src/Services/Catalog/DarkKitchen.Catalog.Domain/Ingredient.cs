namespace DarkKitchen.Catalog.Domain;

public sealed class Ingredient
{
    private Ingredient()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<RecipeItem> RecipeItems { get; private set; } = [];

    public static Ingredient Create(string name, string unit, bool isActive, DateTimeOffset now, Guid? id = null)
    {
        return new Ingredient
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Unit = unit,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, string unit, bool isActive, DateTimeOffset now)
    {
        Name = name;
        Unit = unit;
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }
}
