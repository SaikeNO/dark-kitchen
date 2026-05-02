using DarkKitchen.Testing.Http;

namespace DarkKitchen.Storefront.IntegrationTests;

public sealed class StorefrontApiClient(HttpClient httpClient) : IDisposable
{
    private const string DemoBrandId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001";
    private const string DemoMenuItemId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006";
    private readonly Dictionary<string, string> cookies = new(StringComparer.OrdinalIgnoreCase);

    public static Guid DemoBrandGuid => Guid.Parse(DemoBrandId);
    public static Guid DemoMenuItemGuid => Guid.Parse(DemoMenuItemId);

    public async Task<IReadOnlyList<StorefrontContextResponse>> ListBrandsAsync()
    {
        using var response = await httpClient.GetAsync("/api/storefront/brands");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<IReadOnlyList<StorefrontContextResponse>>();
    }

    public async Task<StorefrontContextResponse> GetContextAsync()
    {
        using var response = await httpClient.GetAsync($"/api/storefront/context?brandId={DemoBrandId}");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<StorefrontContextResponse>();
    }

    public async Task<StorefrontMenuResponse> GetMenuAsync()
    {
        using var response = await httpClient.GetAsync($"/api/storefront/menu?brandId={DemoBrandId}");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<StorefrontMenuResponse>();
    }

    public async Task<CartResponse> CreateCartAsync()
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"/api/storefront/carts?brandId={DemoBrandId}",
            new CreateCartRequest(null),
            HttpTestExtensions.JsonOptions);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CartResponse>();
    }

    public async Task<CartResponse> GetCartAsync(Guid cartId)
    {
        using var response = await httpClient.GetAsync($"/api/storefront/carts/{cartId}?brandId={DemoBrandId}");
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CartResponse>();
    }

    public Task<HttpResponseMessage> GetCartResponseAsync(Guid cartId)
    {
        return httpClient.GetAsync($"/api/storefront/carts/{cartId}?brandId={DemoBrandId}");
    }

    public async Task<CartResponse> UpdateCartAsync(Guid cartId, Guid menuItemId, int quantity)
    {
        using var response = await httpClient.PatchAsJsonAsync(
            $"/api/storefront/carts/{cartId}?brandId={DemoBrandId}",
            new UpdateCartRequest([new CartItemRequest(menuItemId, quantity)]),
            HttpTestExtensions.JsonOptions);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CartResponse>();
    }

    public Task<HttpResponseMessage> UpdateCartResponseAsync(Guid cartId, Guid menuItemId, int quantity)
    {
        return httpClient.PatchAsJsonAsync(
            $"/api/storefront/carts/{cartId}?brandId={DemoBrandId}",
            new UpdateCartRequest([new CartItemRequest(menuItemId, quantity)]),
            HttpTestExtensions.JsonOptions);
    }

    public async Task<CheckoutResponse> CheckoutAsync(Guid cartId, string result)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"/api/storefront/checkout?brandId={DemoBrandId}",
            new CheckoutRequest(
                cartId,
                new CheckoutCustomerRequest("Storefront Test", "500600700", "Integration"),
                result),
            HttpTestExtensions.JsonOptions);
        await response.AssertSuccessAsync();
        return await response.ReadJsonAsync<CheckoutResponse>();
    }

    public async Task<CustomerSessionResponse> RegisterAsync(string email, string password)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/storefront/auth/register",
            new RegisterCustomerRequest(email, password, "Storefront Customer", "500100200"),
            HttpTestExtensions.JsonOptions);
        await response.AssertSuccessAsync();
        ApplyAuthCookies(response);
        return await response.ReadJsonAsync<CustomerSessionResponse>();
    }

    public async Task<CustomerSessionResponse> LoginAsync(string email, string password)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/storefront/auth/login",
            new LoginCustomerRequest(email, password),
            HttpTestExtensions.JsonOptions);
        await response.AssertSuccessAsync();
        ApplyAuthCookies(response);
        return await response.ReadJsonAsync<CustomerSessionResponse>();
    }

    public async Task<CustomerSessionResponse?> GetMeAsync()
    {
        using var response = await httpClient.GetAsync("/api/storefront/auth/me");
        await response.AssertSuccessAsync();
        var body = await response.ReadBodyAsync();
        if (string.IsNullOrWhiteSpace(body) || body.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return await response.ReadJsonAsync<CustomerSessionResponse?>();
    }

    public async Task LogoutAsync()
    {
        using var response = await httpClient.PostAsync("/api/storefront/auth/logout", null);
        await response.AssertSuccessAsync();
        ApplyAuthCookies(response);
    }

    public void Dispose() => httpClient.Dispose();

    private void ApplyAuthCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return;
        }

        foreach (var header in setCookieHeaders)
        {
            var nameValue = header.Split(';', 2)[0];
            var separator = nameValue.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var name = nameValue[..separator];
            var value = nameValue[(separator + 1)..];
            if (string.IsNullOrEmpty(value)
                || header.Contains("max-age=0", StringComparison.OrdinalIgnoreCase)
                || header.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase))
            {
                cookies.Remove(name);
                continue;
            }

            cookies[name] = value;
        }

        httpClient.DefaultRequestHeaders.Remove("Cookie");
        if (cookies.Count > 0)
        {
            var cookieHeader = string.Join("; ", cookies.Select(pair => $"{pair.Key}={pair.Value}"));
            httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
        }
    }
}

public sealed record StorefrontContextResponse(
    Guid BrandId,
    string BrandName,
    string? Description,
    string? LogoUrl,
    string? HeroTitle,
    string? HeroSubtitle,
    StorefrontThemeResponse Theme);

public sealed record StorefrontThemeResponse(string PrimaryColor, string AccentColor, string BackgroundColor, string TextColor);

public sealed record StorefrontMenuResponse(StorefrontContextResponse Brand, IReadOnlyList<StorefrontCategoryResponse> Categories);

public sealed record StorefrontCategoryResponse(Guid Id, string Name, int SortOrder, IReadOnlyList<StorefrontProductResponse> Products);

public sealed record StorefrontProductResponse(Guid Id, Guid CategoryId, string Name, string? Description, string? ImageUrl, decimal Price, string Currency);

public sealed record CreateCartRequest(Guid? CartId);

public sealed record UpdateCartRequest(IReadOnlyList<CartItemRequest> Items);

public sealed record CartItemRequest(Guid MenuItemId, int Quantity);

public sealed record CartResponse(Guid CartId, Guid BrandId, decimal TotalPrice, string Currency, IReadOnlyList<CartItemResponse> Items);

public sealed record CartItemResponse(Guid MenuItemId, string Name, string? ImageUrl, int Quantity, decimal UnitPrice, string Currency, decimal LineTotal);

public sealed record CheckoutRequest(Guid CartId, CheckoutCustomerRequest? Customer, string? MockPaymentResult);

public sealed record CheckoutCustomerRequest(string? DisplayName, string? Phone, string? DeliveryNote);

public sealed record CheckoutResponse(Guid PaymentId, string PaymentStatus, Guid? OrderId, Guid? CorrelationId, string? FailureReason);

public sealed record RegisterCustomerRequest(string Email, string Password, string? DisplayName, string? Phone);

public sealed record LoginCustomerRequest(string Email, string Password);

public sealed record CustomerSessionResponse(Guid Id, string Email, string? DisplayName, string? Phone);
