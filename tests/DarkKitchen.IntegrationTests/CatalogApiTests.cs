using System.Net.Http.Json;
using System.Text.Json;
using DarkKitchen.IntegrationTests.Infrastructure;

namespace DarkKitchen.IntegrationTests;

[Collection(AspireAppCollection.Name)]
public sealed class CatalogApiTests(AspireAppFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Manager_CanCreateCatalogData_AndPublicMenuIsScopedByBrand()
    {
        await fixture.WaitForHealthyAsync("catalog-api");

        using var client = fixture.CreateHttpClient("catalog-api");
        await LoginAsync(client, "manager@darkkitchen.local", "Demo123!");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var brandA = await CreateBrandAsync(client, $"Slice Burger {suffix}");
        var brandB = await CreateBrandAsync(client, $"Slice Pizza {suffix}");
        var categoryA = await CreateCategoryAsync(client, brandA.Id, $"Burgers {suffix}");
        var categoryB = await CreateCategoryAsync(client, brandB.Id, $"Pizza {suffix}");
        var ingredient = await CreateIngredientAsync(client, $"Ingredient {suffix}");
        var station = await CreateStationAsync(client, $"GR{suffix[..4]}", $"Grill {suffix}");
        var productA = await CreateProductAsync(client, brandA.Id, categoryA.Id, $"Smash {suffix}", 31.50m);
        var productB = await CreateProductAsync(client, brandB.Id, categoryB.Id, $"Margherita {suffix}", 29.00m);

        await UpsertRecipeAsync(client, productA.Id, ingredient.Id);
        await UpsertRouteAsync(client, productA.Id, station.Id);
        await ActivateProductAsync(client, productA.Id);
        await UpsertRecipeAsync(client, productB.Id, ingredient.Id);
        await UpsertRouteAsync(client, productB.Id, station.Id);
        await ActivateProductAsync(client, productB.Id);

        var menu = await GetJsonAsync<MenuResponse>(client, $"/api/menu/brands/{brandA.Id}");

        Assert.Equal(brandA.Id, menu.BrandId);
        var menuProducts = menu.Categories.SelectMany(category => category.Products).ToArray();
        Assert.Contains(menuProducts, product => product.Id == productA.Id);
        Assert.DoesNotContain(menuProducts, product => product.Id == productB.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Operator_CannotMutateCatalogData()
    {
        await fixture.WaitForHealthyAsync("catalog-api");

        using var client = fixture.CreateHttpClient("catalog-api");
        await LoginAsync(client, "operator@darkkitchen.local", "Demo123!");

        using var response = await client.PostAsJsonAsync(
            "/api/admin/brands",
            new BrandRequest($"Operator Brand {Guid.NewGuid():N}", null, null, true),
            JsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ProductActivation_WithoutRecipeAndStation_ReturnsValidationProblem()
    {
        await fixture.WaitForHealthyAsync("catalog-api");

        using var client = fixture.CreateHttpClient("catalog-api");
        await LoginAsync(client, "manager@darkkitchen.local", "Demo123!");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var brand = await CreateBrandAsync(client, $"Validation Brand {suffix}");
        var category = await CreateCategoryAsync(client, brand.Id, $"Validation Category {suffix}");
        var product = await CreateProductAsync(client, brand.Id, category.Id, $"Validation Product {suffix}", 20.00m);

        using var response = await client.PostAsync($"/api/admin/products/{product.Id}/activate", null);
        var problemJson = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Recipe with at least one ingredient is required.", problemJson, StringComparison.Ordinal);
        Assert.Contains("Active kitchen station route is required.", problemJson, StringComparison.Ordinal);
    }

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/auth/login",
            new LoginRequest(email, password),
            JsonOptions);

        await AssertSuccessAsync(response);

        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            Assert.Fail("Login response did not set an auth cookie.");
        }

        var cookieHeader = string.Join("; ", setCookieHeaders.Select(header => header.Split(';')[0]));
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
    }

    private static async Task<BrandResponse> CreateBrandAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/brands",
            new BrandRequest(name, "Integration test brand", null, true),
            JsonOptions);

        await AssertSuccessAsync(response);
        return await ReadJsonAsync<BrandResponse>(response);
    }

    private static async Task<CategoryResponse> CreateCategoryAsync(HttpClient client, Guid brandId, string name)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/categories",
            new CategoryRequest(brandId, name, 10, true),
            JsonOptions);

        await AssertSuccessAsync(response);
        return await ReadJsonAsync<CategoryResponse>(response);
    }

    private static async Task<IngredientResponse> CreateIngredientAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/ingredients",
            new IngredientRequest(name, "g", true),
            JsonOptions);

        await AssertSuccessAsync(response);
        return await ReadJsonAsync<IngredientResponse>(response);
    }

    private static async Task<StationResponse> CreateStationAsync(HttpClient client, string code, string name)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/stations",
            new StationRequest(code, name, "#2f7d57", true),
            JsonOptions);

        await AssertSuccessAsync(response);
        return await ReadJsonAsync<StationResponse>(response);
    }

    private static async Task<ProductResponse> CreateProductAsync(
        HttpClient client,
        Guid brandId,
        Guid categoryId,
        string name,
        decimal price)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/admin/products",
            new ProductRequest(brandId, categoryId, name, "Integration product", price, "PLN"),
            JsonOptions);

        await AssertSuccessAsync(response);
        return await ReadJsonAsync<ProductResponse>(response);
    }

    private static async Task UpsertRecipeAsync(HttpClient client, Guid productId, Guid ingredientId)
    {
        using var response = await client.PutAsJsonAsync(
            $"/api/admin/products/{productId}/recipe",
            new RecipeRequest([new RecipeItemRequest(ingredientId, 1)]),
            JsonOptions);

        await AssertSuccessAsync(response);
    }

    private static async Task UpsertRouteAsync(HttpClient client, Guid productId, Guid stationId)
    {
        using var response = await client.PutAsJsonAsync(
            $"/api/admin/products/{productId}/station-route",
            new RouteRequest(stationId),
            JsonOptions);

        await AssertSuccessAsync(response);
    }

    private static async Task ActivateProductAsync(HttpClient client, Guid productId)
    {
        using var response = await client.PostAsync($"/api/admin/products/{productId}/activate", null);
        await AssertSuccessAsync(response);
    }

    private static async Task<T> GetJsonAsync<T>(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        await AssertSuccessAsync(response);
        return await ReadJsonAsync<T>(response);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return value ?? throw new InvalidOperationException("Response body was empty.");
    }

    private static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success but got {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    private sealed record LoginRequest(string Email, string Password);

    private sealed record BrandRequest(string Name, string? Description, string? LogoUrl, bool IsActive);

    private sealed record CategoryRequest(Guid BrandId, string Name, int SortOrder, bool IsActive);

    private sealed record IngredientRequest(string Name, string Unit, bool IsActive);

    private sealed record StationRequest(string Code, string Name, string DisplayColor, bool IsActive);

    private sealed record ProductRequest(
        Guid BrandId,
        Guid CategoryId,
        string Name,
        string? Description,
        decimal Price,
        string Currency);

    private sealed record RecipeRequest(IReadOnlyList<RecipeItemRequest> Items);

    private sealed record RecipeItemRequest(Guid IngredientId, decimal Quantity);

    private sealed record RouteRequest(Guid StationId);

    private sealed record BrandResponse(Guid Id, string Name);

    private sealed record CategoryResponse(Guid Id, Guid BrandId, string Name);

    private sealed record IngredientResponse(Guid Id, string Name);

    private sealed record StationResponse(Guid Id, string Code);

    private sealed record ProductResponse(Guid Id, Guid BrandId, Guid CategoryId, string Name);

    private sealed record MenuResponse(Guid BrandId, IReadOnlyList<MenuCategoryResponse> Categories);

    private sealed record MenuCategoryResponse(Guid Id, string Name, IReadOnlyList<MenuProductResponse> Products);

    private sealed record MenuProductResponse(Guid Id, string Name);
}
