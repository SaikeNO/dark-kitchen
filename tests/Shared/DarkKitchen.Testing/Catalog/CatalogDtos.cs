namespace DarkKitchen.Testing.Catalog;

public sealed record LoginRequest(string Email, string Password);

public sealed record AdminUserResponse(string Email, IReadOnlyList<string> Roles);

public sealed record BrandRequest(string Name, string? Description, string? LogoUrl, bool IsActive);

public sealed record BrandResponse(Guid Id, string Name, string? Description, string? LogoUrl, bool IsActive);

public sealed record CategoryRequest(Guid BrandId, string Name, int SortOrder, bool IsActive);

public sealed record CategoryResponse(Guid Id, Guid BrandId, string Name, int SortOrder, bool IsActive);

public sealed record IngredientRequest(string Name, string Unit, bool IsActive);

public sealed record IngredientResponse(Guid Id, string Name, string Unit, bool IsActive);

public sealed record StationRequest(string Code, string Name, string DisplayColor, bool IsActive);

public sealed record StationResponse(Guid Id, string Code, string Name, string DisplayColor, bool IsActive);

public sealed record ProductRequest(
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency);

public sealed record ProductResponse(
    Guid Id,
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    Guid? StationId,
    string? StationCode,
    int RecipeItemCount);

public sealed record RecipeRequest(IReadOnlyList<RecipeItemRequest> Items);

public sealed record RecipeItemRequest(Guid IngredientId, decimal Quantity);

public sealed record RecipeResponse(Guid ProductId, IReadOnlyList<RecipeItemResponse> Items);

public sealed record RecipeItemResponse(Guid IngredientId, string IngredientName, string Unit, decimal Quantity);

public sealed record ProductStationRouteRequest(Guid StationId);

public sealed record ProductStationRouteResponse(Guid ProductId, Guid StationId, string StationCode);

public sealed record MenuResponse(Guid BrandId, string BrandName, IReadOnlyList<MenuCategoryResponse> Categories);

public sealed record MenuCategoryResponse(Guid Id, string Name, int SortOrder, IReadOnlyList<MenuProductResponse> Products);

public sealed record MenuProductResponse(Guid Id, string Name, string? Description, decimal Price, string Currency);

public sealed record CatalogProductScenario(
    BrandResponse Brand,
    CategoryResponse Category,
    IngredientResponse Ingredient,
    StationResponse Station,
    ProductResponse Product);
