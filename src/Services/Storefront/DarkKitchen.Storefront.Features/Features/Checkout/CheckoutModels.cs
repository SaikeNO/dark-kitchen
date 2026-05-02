namespace DarkKitchen.Storefront.Features.Features.Checkout;

public sealed record CheckoutRequest(
    Guid CartId,
    CheckoutCustomerRequest? Customer,
    string? MockPaymentResult);

public sealed record CheckoutCustomerRequest(
    string? DisplayName,
    string? Phone,
    string? DeliveryNote);

public sealed record CheckoutResponse(
    Guid PaymentId,
    string PaymentStatus,
    Guid? OrderId,
    Guid? CorrelationId,
    string? FailureReason);
