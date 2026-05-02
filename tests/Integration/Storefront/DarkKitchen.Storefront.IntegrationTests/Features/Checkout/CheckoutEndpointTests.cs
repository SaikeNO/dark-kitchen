namespace DarkKitchen.Storefront.IntegrationTests.Features.Checkout;

[Collection(AspireAppCollection.Name)]
public sealed class CheckoutEndpointTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task SuccessPayment_SubmitsOrderToOms()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        await fixture.WaitForHealthyAsync("order-management-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var cart = await client.CreateCartAsync();
        await client.UpdateCartAsync(cart.CartId, StorefrontApiClient.DemoMenuItemGuid, 1);

        var checkout = await client.CheckoutAsync(cart.CartId, "success");

        Assert.Equal("Success", checkout.PaymentStatus);
        Assert.NotNull(checkout.OrderId);
        Assert.NotNull(checkout.CorrelationId);
    }

    [Fact]
    public async Task FailedPayment_DoesNotCreateOrder()
    {
        await fixture.WaitForHealthyAsync("storefront-api");
        using var client = new StorefrontApiClient(fixture.CreateHttpClient("storefront-api"));
        var cart = await client.CreateCartAsync();
        await client.UpdateCartAsync(cart.CartId, StorefrontApiClient.DemoMenuItemGuid, 1);

        var checkout = await client.CheckoutAsync(cart.CartId, "failed");

        Assert.Equal("Failed", checkout.PaymentStatus);
        Assert.Null(checkout.OrderId);
        Assert.Equal("mock_payment_failed", checkout.FailureReason);
    }
}
