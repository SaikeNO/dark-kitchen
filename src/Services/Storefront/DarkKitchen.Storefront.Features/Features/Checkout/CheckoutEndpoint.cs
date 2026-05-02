using DarkKitchen.Storefront.Features.Features.Carts;

namespace DarkKitchen.Storefront.Features.Features.Checkout;

public static class CheckoutEndpoint
{
    public static async Task<IResult> HandleAsync(
        CheckoutRequest request,
        StorefrontDbContext db,
        HttpContext httpContext,
        OrderManagementClient orderManagement,
        CancellationToken ct)
    {
        var brand = await BrandResolver.ResolveAsync(httpContext, db, ct);
        if (brand is null)
        {
            return Results.NotFound();
        }

        var cart = await CartAccess.FindCartAsync(db, request.CartId, brand.BrandId, ct);
        if (cart is null)
        {
            return Results.NotFound();
        }

        if (cart.Items.Count == 0)
        {
            return ApiValidation.Problem(("cartId", "Cart is empty."));
        }

        var now = DateTimeOffset.UtcNow;
        var payment = PaymentTransaction.Create(brand.BrandId, cart.Id, cart.TotalPrice, cart.Currency, now);
        db.PaymentTransactions.Add(payment);
        await db.SaveChangesAsync(ct);

        if (IsPaymentFailure(request.MockPaymentResult))
        {
            payment.MarkFailed("mock_payment_failed", DateTimeOffset.UtcNow);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new CheckoutResponse(payment.Id, payment.Status.ToString(), null, null, payment.FailureReason));
        }

        var correlationId = Guid.NewGuid();
        var order = await orderManagement.SubmitStorefrontOrderAsync(
            new SubmitOrderRequest(
                brand.BrandId,
                $"sf-{cart.Id:N}-{payment.Id:N}",
                NormalizeCustomer(request.Customer),
                cart.Items.Select(item => new SubmitOrderItemRequest(item.MenuItemId, item.Quantity)).ToArray()),
            correlationId,
            ct);

        payment.MarkSuccess(order.OrderId, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new CheckoutResponse(payment.Id, payment.Status.ToString(), order.OrderId, order.CorrelationId, null));
    }

    private static bool IsPaymentFailure(string? value)
    {
        return string.Equals(value, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "fail", StringComparison.OrdinalIgnoreCase);
    }

    private static CheckoutCustomerRequest? NormalizeCustomer(CheckoutCustomerRequest? customer)
    {
        if (customer is null)
        {
            return null;
        }

        return new CheckoutCustomerRequest(
            ApiValidation.TrimOptional(customer.DisplayName),
            ApiValidation.TrimOptional(customer.Phone),
            ApiValidation.TrimOptional(customer.DeliveryNote));
    }
}
