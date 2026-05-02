using DarkKitchen.Testing.Http;

namespace DarkKitchen.Inventory.IntegrationTests;

public sealed class InventoryApiClient(HttpClient httpClient) : IDisposable
{
    public async Task<IReadOnlyList<InventoryItemResponse>> ListItemsAsync()
    {
        using var response = await httpClient.GetAsync("/api/admin/inventory/items");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<InventoryItemResponse>>();
    }

    public async Task<IReadOnlyList<InventoryItemResponse>> ListShortagesAsync()
    {
        using var response = await httpClient.GetAsync("/api/admin/inventory/shortages");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<InventoryItemResponse>>();
    }

    public Task<HttpResponseMessage> PostDeliveryAsync(Guid ingredientId, DeliveryRequest request)
    {
        return httpClient.PostAsJsonAsync($"/api/admin/inventory/items/{ingredientId}/delivery", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<InventoryItemResponse> RecordDeliveryAsync(Guid ingredientId, decimal quantity)
    {
        using var response = await PostDeliveryAsync(ingredientId, new DeliveryRequest(quantity, null));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<InventoryItemResponse>();
    }

    public Task<HttpResponseMessage> PostAdjustmentAsync(Guid ingredientId, AdjustmentRequest request)
    {
        return httpClient.PostAsJsonAsync($"/api/admin/inventory/items/{ingredientId}/adjustment", request, HttpTestExtensions.JsonOptions);
    }

    public async Task<InventoryItemResponse> AdjustAsync(Guid ingredientId, decimal onHandQuantity, decimal minSafetyLevel)
    {
        using var response = await PostAdjustmentAsync(ingredientId, new AdjustmentRequest(onHandQuantity, minSafetyLevel, null));
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<InventoryItemResponse>();
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}

public sealed record InventoryItemResponse(
    Guid IngredientId,
    string Name,
    string Unit,
    decimal OnHandQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal MinSafetyLevel,
    bool IsBelowSafetyLevel,
    decimal ReorderQuantity);

public sealed record DeliveryRequest(decimal Quantity, string? Note);

public sealed record AdjustmentRequest(decimal OnHandQuantity, decimal? MinSafetyLevel, string? Note);
