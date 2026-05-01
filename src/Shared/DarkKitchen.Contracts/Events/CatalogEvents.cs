namespace DarkKitchen.Contracts.Events;

public sealed record MenuItemChanged(
    Guid ProductId,
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive);

public sealed record ProductPriceChanged(
    Guid ProductId,
    Guid BrandId,
    decimal Price,
    string Currency);

public sealed record RecipeChanged(
    Guid ProductId,
    Guid BrandId,
    IReadOnlyList<RecipeChangedItem> Items);

public sealed record RecipeChangedItem(
    Guid IngredientId,
    string Name,
    string Unit,
    decimal Quantity);

public sealed record StationChanged(
    Guid StationId,
    string Code,
    string Name,
    string DisplayColor,
    bool IsActive);

public sealed record ProductStationRoutingChanged(
    Guid ProductId,
    Guid BrandId,
    Guid? StationId,
    string? StationCode);
