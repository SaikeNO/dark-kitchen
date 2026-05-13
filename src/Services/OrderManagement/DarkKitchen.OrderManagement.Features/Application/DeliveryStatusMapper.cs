namespace DarkKitchen.OrderManagement.Features.Application;

public static class DeliveryStatusMapper
{
    public static bool IsOrderCreationStatus(string? externalStatus)
    {
        if (string.IsNullOrWhiteSpace(externalStatus))
        {
            return true;
        }

        var normalized = externalStatus.Trim().ToLowerInvariant();
        return normalized is "created" or "placed";
    }
}
