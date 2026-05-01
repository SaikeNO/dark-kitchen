namespace DarkKitchen.Catalog.Api;

public sealed class Brand
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}

public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BrandId { get; set; }
    public Brand? Brand { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Product> Products { get; set; } = [];
}

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "PLN";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Recipe? Recipe { get; set; }
    public ProductStationRoute? StationRoute { get; set; }
}

public sealed class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<RecipeItem> RecipeItems { get; set; } = [];
}

public sealed class Recipe
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<RecipeItem> Items { get; set; } = [];
}

public sealed class RecipeItem
{
    public Guid ProductId { get; set; }
    public Recipe? Recipe { get; set; }
    public Guid IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class Station
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string DisplayColor { get; set; } = "#3f7f5f";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ProductStationRoute> Routes { get; set; } = [];
}

public sealed class ProductStationRoute
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid StationId { get; set; }
    public Station? Station { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
