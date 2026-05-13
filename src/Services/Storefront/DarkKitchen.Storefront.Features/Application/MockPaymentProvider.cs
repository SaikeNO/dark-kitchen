namespace DarkKitchen.Storefront.Features.Application;

public sealed class MockPaymentProvider : IPaymentProvider
{
    private const string FailureReason = "mock_payment_failed";

    public Task<PaymentInitiationResult> InitiateAsync(PaymentInitiationRequest request, CancellationToken ct)
    {
        var externalTransactionId = $"mock_{request.PaymentId:N}";
        return Task.FromResult(new PaymentInitiationResult(externalTransactionId));
    }

    public Task<PaymentConfirmationResult> ConfirmAsync(PaymentConfirmationRequest request, CancellationToken ct)
    {
        var failed = string.Equals(request.RequestedResult, "failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.RequestedResult, "fail", StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(failed
            ? new PaymentConfirmationResult(false, FailureReason)
            : new PaymentConfirmationResult(true, null));
    }
}
