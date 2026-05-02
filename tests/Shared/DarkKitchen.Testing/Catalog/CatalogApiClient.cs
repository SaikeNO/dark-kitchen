using DarkKitchen.Testing.Http;

namespace DarkKitchen.Testing.Catalog;

public sealed class CatalogApiClient(HttpClient httpClient) : IDisposable
{
    private readonly HttpClient _httpClient = httpClient;

    public HttpClient HttpClient => _httpClient;

    public Task<HttpResponseMessage> PostLoginAsync(LoginRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/auth/login", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<AdminUserResponse> LoginAsManagerAsync()
    {
        return await LoginAsync("manager@darkkitchen.local", "Demo123!");
    }

    public async Task<AdminUserResponse> LoginAsOperatorAsync()
    {
        return await LoginAsync("operator@darkkitchen.local", "Demo123!");
    }

    public async Task<AdminUserResponse> LoginAsync(string email, string password)
    {
        using var response = await PostLoginAsync(new LoginRequest(email, password));
        await response.AssertSuccessAsync();
        ApplyAuthCookies(response);
        return await response.ReadJsonAsync<AdminUserResponse>();
    }

    public async Task<AdminUserResponse> GetCurrentUserAsync()
    {
        using var response = await _httpClient.GetAsync("/api/admin/auth/me");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<AdminUserResponse>();
    }

    public Task<HttpResponseMessage> PostLogoutAsync()
    {
        return _httpClient.PostAsync("/api/admin/auth/logout", null);
    }

    public Task<HttpResponseMessage> PostBrandAsync(BrandRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/brands", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<BrandResponse> CreateBrandAsync(string name, bool isActive = true)
    {
        using var response = await PostBrandAsync(new BrandRequest(
            name,
            "Integration test brand",
            null,
            [],
            $"{name} hero",
            "Integration hero",
            "#dc2626",
            "#ca8a04",
            "#fef2f2",
            "#450a0a",
            isActive));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<BrandResponse>();
    }

    public async Task<IReadOnlyList<BrandResponse>> ListBrandsAsync()
    {
        using var response = await _httpClient.GetAsync("/api/admin/brands");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<BrandResponse>>();
    }

    public Task<HttpResponseMessage> PutBrandAsync(Guid brandId, BrandRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/brands/{brandId}", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<BrandResponse> UpdateBrandAsync(Guid brandId, BrandRequest request)
    {
        using var response = await PutBrandAsync(brandId, request);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<BrandResponse>();
    }

    public async Task<BrandResponse> DeactivateBrandAsync(Guid brandId)
    {
        using var response = await PostDeactivateBrandAsync(brandId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<BrandResponse>();
    }

    public Task<HttpResponseMessage> PostDeactivateBrandAsync(Guid brandId)
    {
        return _httpClient.PostAsync($"/api/admin/brands/{brandId}/deactivate", null);
    }

    public Task<HttpResponseMessage> PostCategoryAsync(CategoryRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/categories", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(Guid brandId, string name, bool isActive = true)
    {
        using var response = await PostCategoryAsync(new CategoryRequest(brandId, name, 10, isActive));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CategoryResponse>();
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListCategoriesAsync(Guid? brandId = null)
    {
        var path = brandId.HasValue ? $"/api/admin/categories?brandId={brandId.Value}" : "/api/admin/categories";
        using var response = await _httpClient.GetAsync(path);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<CategoryResponse>>();
    }

    public Task<HttpResponseMessage> PutCategoryAsync(Guid categoryId, CategoryRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/categories/{categoryId}", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CategoryRequest request)
    {
        using var response = await PutCategoryAsync(categoryId, request);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CategoryResponse>();
    }

    public async Task<CategoryResponse> DeactivateCategoryAsync(Guid categoryId)
    {
        using var response = await PostDeactivateCategoryAsync(categoryId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CategoryResponse>();
    }

    public Task<HttpResponseMessage> PostDeactivateCategoryAsync(Guid categoryId)
    {
        return _httpClient.PostAsync($"/api/admin/categories/{categoryId}/deactivate", null);
    }

    public Task<HttpResponseMessage> PostIngredientAsync(IngredientRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/ingredients", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<IngredientResponse> CreateIngredientAsync(string name, bool isActive = true)
    {
        using var response = await PostIngredientAsync(new IngredientRequest(name, "g", isActive));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IngredientResponse>();
    }

    public async Task<IReadOnlyList<IngredientResponse>> ListIngredientsAsync()
    {
        using var response = await _httpClient.GetAsync("/api/admin/ingredients");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<IngredientResponse>>();
    }

    public Task<HttpResponseMessage> PutIngredientAsync(Guid ingredientId, IngredientRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/ingredients/{ingredientId}", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<IngredientResponse> UpdateIngredientAsync(Guid ingredientId, IngredientRequest request)
    {
        using var response = await PutIngredientAsync(ingredientId, request);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IngredientResponse>();
    }

    public async Task<IngredientResponse> DeactivateIngredientAsync(Guid ingredientId)
    {
        using var response = await PostDeactivateIngredientAsync(ingredientId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IngredientResponse>();
    }

    public Task<HttpResponseMessage> PostDeactivateIngredientAsync(Guid ingredientId)
    {
        return _httpClient.PostAsync($"/api/admin/ingredients/{ingredientId}/deactivate", null);
    }

    public Task<HttpResponseMessage> PostStationAsync(StationRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/stations", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<StationResponse> CreateStationAsync(string code, string name, bool isActive = true)
    {
        using var response = await PostStationAsync(new StationRequest(code, name, "#2f7d57", isActive));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<StationResponse>();
    }

    public async Task<IReadOnlyList<StationResponse>> ListStationsAsync()
    {
        using var response = await _httpClient.GetAsync("/api/admin/stations");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<StationResponse>>();
    }

    public Task<HttpResponseMessage> PutStationAsync(Guid stationId, StationRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/stations/{stationId}", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<StationResponse> UpdateStationAsync(Guid stationId, StationRequest request)
    {
        using var response = await PutStationAsync(stationId, request);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<StationResponse>();
    }

    public async Task<StationResponse> DeactivateStationAsync(Guid stationId)
    {
        using var response = await PostDeactivateStationAsync(stationId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<StationResponse>();
    }

    public Task<HttpResponseMessage> PostDeactivateStationAsync(Guid stationId)
    {
        return _httpClient.PostAsync($"/api/admin/stations/{stationId}/deactivate", null);
    }

    public Task<HttpResponseMessage> PostProductAsync(ProductRequest request)
    {
        return _httpClient.PostAsJsonAsync("/api/admin/products", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<ProductResponse> CreateProductAsync(
        Guid brandId,
        Guid categoryId,
        string name,
        decimal price = 31.50m)
    {
        using var response = await PostProductAsync(new ProductRequest(brandId, categoryId, name, "Integration product", null, price, "PLN"));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<ProductResponse>();
    }

    public async Task<IReadOnlyList<ProductResponse>> ListProductsAsync(Guid? brandId = null)
    {
        var path = brandId.HasValue ? $"/api/admin/products?brandId={brandId.Value}" : "/api/admin/products";
        using var response = await _httpClient.GetAsync(path);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<ProductResponse>>();
    }

    public Task<HttpResponseMessage> PutProductAsync(Guid productId, ProductRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/products/{productId}", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid productId, ProductRequest request)
    {
        using var response = await PutProductAsync(productId, request);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<ProductResponse>();
    }

    public async Task<ProductResponse> ActivateProductAsync(Guid productId)
    {
        using var response = await PostActivateProductAsync(productId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<ProductResponse>();
    }

    public Task<HttpResponseMessage> PostActivateProductAsync(Guid productId)
    {
        return _httpClient.PostAsync($"/api/admin/products/{productId}/activate", null);
    }

    public async Task<ProductResponse> DeactivateProductAsync(Guid productId)
    {
        using var response = await PostDeactivateProductAsync(productId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<ProductResponse>();
    }

    public Task<HttpResponseMessage> PostDeactivateProductAsync(Guid productId)
    {
        return _httpClient.PostAsync($"/api/admin/products/{productId}/deactivate", null);
    }

    public async Task<RecipeResponse> GetRecipeAsync(Guid productId)
    {
        using var response = await GetRecipeResponseAsync(productId);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<RecipeResponse>();
    }

    public Task<HttpResponseMessage> GetRecipeResponseAsync(Guid productId)
    {
        return _httpClient.GetAsync($"/api/admin/products/{productId}/recipe");
    }

    public Task<HttpResponseMessage> PutRecipeAsync(Guid productId, RecipeRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/products/{productId}/recipe", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<RecipeResponse> UpsertRecipeAsync(Guid productId, Guid ingredientId, decimal quantity = 1)
    {
        using var response = await PutRecipeAsync(productId, new RecipeRequest([new RecipeItemRequest(ingredientId, quantity)]));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<RecipeResponse>();
    }

    public Task<HttpResponseMessage> PutStationRouteAsync(Guid productId, ProductStationRouteRequest request)
    {
        return _httpClient.PutAsJsonAsync($"/api/admin/products/{productId}/station-route", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<ProductStationRouteResponse> UpsertStationRouteAsync(Guid productId, Guid stationId)
    {
        using var response = await PutStationRouteAsync(productId, new ProductStationRouteRequest(stationId));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<ProductStationRouteResponse>();
    }

    public async Task<MenuResponse> GetMenuAsync(Guid brandId)
    {
        using var response = await _httpClient.GetAsync($"/api/menu/brands/{brandId}");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<MenuResponse>();
    }

    public async Task<CatalogProductScenario> CreateProductScenarioAsync(string suffix)
    {
        var brand = await CreateBrandAsync($"Scenario Brand {suffix}");
        var category = await CreateCategoryAsync(brand.Id, $"Scenario Category {suffix}");
        var ingredient = await CreateIngredientAsync($"Scenario Ingredient {suffix}");
        var station = await CreateStationAsync($"ST{suffix[..4]}", $"Scenario Station {suffix}");
        var product = await CreateProductAsync(brand.Id, category.Id, $"Scenario Product {suffix}");

        return new CatalogProductScenario(brand, category, ingredient, station, product);
    }

    public async Task<ProductResponse> CreateActivatableProductAsync(string suffix)
    {
        var scenario = await CreateProductScenarioAsync(suffix);
        await UpsertRecipeAsync(scenario.Product.Id, scenario.Ingredient.Id);
        await UpsertStationRouteAsync(scenario.Product.Id, scenario.Station.Id);
        return scenario.Product;
    }

    private void ApplyAuthCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            Assert.Fail("Login response did not set an auth cookie.");
        }

        var cookieHeader = string.Join("; ", setCookieHeaders.Select(header => header.Split(';')[0]));
        _httpClient.DefaultRequestHeaders.Remove("Cookie");
        _httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
