namespace DarkKitchen.Inventory.Domain;

public sealed class WarehouseItem
{
    private WarehouseItem()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal OnHandQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal MinSafetyLevel { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public decimal AvailableQuantity => OnHandQuantity - ReservedQuantity;
    public bool IsBelowSafetyLevel => AvailableQuantity < MinSafetyLevel;
    public decimal ReorderQuantity => IsBelowSafetyLevel ? MinSafetyLevel - AvailableQuantity : 0;

    public static WarehouseItem Create(
        Guid ingredientId,
        string name,
        string unit,
        DateTimeOffset now,
        decimal onHandQuantity = 0,
        decimal minSafetyLevel = 0)
    {
        if (ingredientId == Guid.Empty)
        {
            throw new ArgumentException("Ingredient id is required.", nameof(ingredientId));
        }

        if (onHandQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(onHandQuantity), onHandQuantity, "On-hand quantity cannot be negative.");
        }

        if (minSafetyLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minSafetyLevel), minSafetyLevel, "Minimum safety level cannot be negative.");
        }

        return new WarehouseItem
        {
            Id = ingredientId,
            Name = RequireNonWhiteSpace(name, nameof(name)),
            Unit = RequireNonWhiteSpace(unit, nameof(unit)),
            OnHandQuantity = onHandQuantity,
            MinSafetyLevel = minSafetyLevel,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateCatalogData(string name, string unit, DateTimeOffset now)
    {
        Name = RequireNonWhiteSpace(name, nameof(name));
        Unit = RequireNonWhiteSpace(unit, nameof(unit));
        UpdatedAt = now;
    }

    public void ReceiveDelivery(decimal quantity, DateTimeOffset now)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Delivery quantity must be positive.");
        }

        OnHandQuantity += quantity;
        UpdatedAt = now;
    }

    public void Adjust(decimal onHandQuantity, decimal? minSafetyLevel, DateTimeOffset now)
    {
        if (onHandQuantity < ReservedQuantity)
        {
            throw new ArgumentOutOfRangeException(nameof(onHandQuantity), onHandQuantity, "On-hand quantity cannot be lower than reserved quantity.");
        }

        if (minSafetyLevel is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minSafetyLevel), minSafetyLevel, "Minimum safety level cannot be negative.");
        }

        OnHandQuantity = onHandQuantity;
        if (minSafetyLevel.HasValue)
        {
            MinSafetyLevel = minSafetyLevel.Value;
        }

        UpdatedAt = now;
    }

    public void Reserve(decimal quantity, DateTimeOffset now)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Reservation quantity must be positive.");
        }

        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException("Insufficient available quantity.");
        }

        ReservedQuantity += quantity;
        UpdatedAt = now;
    }

    private static string RequireNonWhiteSpace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
