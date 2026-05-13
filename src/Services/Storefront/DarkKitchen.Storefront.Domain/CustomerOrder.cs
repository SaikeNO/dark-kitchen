namespace DarkKitchen.Storefront.Domain;

public sealed class CustomerOrder
{
    private CustomerOrder()
    {
    }

    public Guid OrderId { get; private set; }
    public Guid BrandId { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid CartId { get; private set; }
    public Guid PaymentTransactionId { get; private set; }
    public string Status { get; private set; } = "Placed";
    public string? FailureReason { get; private set; }
    public string? PickupCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static CustomerOrder Create(
        Guid orderId,
        Guid brandId,
        Guid? userId,
        Guid cartId,
        Guid paymentTransactionId,
        DateTimeOffset now)
    {
        return new CustomerOrder
        {
            OrderId = RequireId(orderId, nameof(orderId)),
            BrandId = RequireId(brandId, nameof(brandId)),
            UserId = userId,
            CartId = RequireId(cartId, nameof(cartId)),
            PaymentTransactionId = RequireId(paymentTransactionId, nameof(paymentTransactionId)),
            Status = "Placed",
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void ApplyStatus(string status, DateTimeOffset now, string? failureReason = null, string? pickupCode = null)
    {
        Status = RequireText(status, nameof(status));
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();
        PickupCode = string.IsNullOrWhiteSpace(pickupCode) ? PickupCode : pickupCode.Trim();
        UpdatedAt = now;
    }

    private static Guid RequireId(Guid id, string parameterName)
    {
        return id == Guid.Empty
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : id;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }
}
