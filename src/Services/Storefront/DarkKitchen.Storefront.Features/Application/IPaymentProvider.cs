namespace DarkKitchen.Storefront.Features.Application;

public interface IPaymentProvider
{
    Task<PaymentInitiationResult> InitiateAsync(PaymentInitiationRequest request, CancellationToken ct);

    Task<PaymentConfirmationResult> ConfirmAsync(PaymentConfirmationRequest request, CancellationToken ct);
}

public sealed record PaymentInitiationRequest(
    Guid BrandId,
    Guid CartId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string? RequestedResult);

public sealed record PaymentInitiationResult(string ExternalTransactionId);

public sealed record PaymentConfirmationRequest(
    string ExternalTransactionId,
    string? RequestedResult);

public sealed record PaymentConfirmationResult(
    bool IsSuccess,
    string? FailureReason);
