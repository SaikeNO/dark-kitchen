using DarkKitchen.Storefront.Features.Features.Carts;
using System.Security.Claims;

namespace DarkKitchen.Storefront.Features.Features.Checkout;

public static class CheckoutEndpoint
{
    public static async Task<IResult> HandleAsync(
        CheckoutRequest request,
        StorefrontDbContext db,
        HttpContext httpContext,
        IPaymentProvider paymentProvider,
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

        var initiation = await paymentProvider.InitiateAsync(
            new PaymentInitiationRequest(
                brand.BrandId,
                cart.Id,
                payment.Id,
                cart.TotalPrice,
                cart.Currency,
                request.MockPaymentResult),
            ct);
        payment.SetExternalTransactionId(initiation.ExternalTransactionId, DateTimeOffset.UtcNow);

        var confirmation = await paymentProvider.ConfirmAsync(
            new PaymentConfirmationRequest(initiation.ExternalTransactionId, request.MockPaymentResult),
            ct);

        if (!confirmation.IsSuccess)
        {
            payment.MarkFailed(confirmation.FailureReason ?? "payment_failed", DateTimeOffset.UtcNow);
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
        db.CustomerOrders.Add(CustomerOrder.Create(
            order.OrderId,
            brand.BrandId,
            UserIdFrom(httpContext.User),
            cart.Id,
            payment.Id,
            DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(ct);

        return Results.Ok(new CheckoutResponse(payment.Id, payment.Status.ToString(), order.OrderId, order.CorrelationId, null));
    }

    private static Guid? UserIdFrom(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
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
