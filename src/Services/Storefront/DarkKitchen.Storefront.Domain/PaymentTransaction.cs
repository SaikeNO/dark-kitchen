namespace DarkKitchen.Storefront.Domain;

public sealed class PaymentTransaction
{
    private PaymentTransaction()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BrandId { get; private set; }
    public Guid CartId { get; private set; }
    public Guid? OrderId { get; private set; }
    public string ExternalTransactionId { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PLN";
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static PaymentTransaction Create(Guid brandId, Guid cartId, decimal amount, string currency, DateTimeOffset now)
    {
        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            BrandId = brandId,
            CartId = cartId,
            ExternalTransactionId = $"mock_{Guid.NewGuid():N}",
            Status = PaymentStatus.Pending,
            Amount = amount,
            Currency = currency,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MarkSuccess(Guid orderId, DateTimeOffset now)
    {
        Status = PaymentStatus.Success;
        OrderId = orderId;
        FailureReason = null;
        UpdatedAt = now;
    }

    public void MarkFailed(string reason, DateTimeOffset now)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = now;
    }
}
