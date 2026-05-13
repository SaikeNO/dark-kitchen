using DarkKitchen.OrderManagement.Features.Features.Orders;

namespace DarkKitchen.OrderManagement.Features.Application;

public sealed class MockDeliveryOrderAdapter : IDeliveryOrderAdapter
{
    private static readonly HashSet<string> SupportedPlatforms = new(StringComparer.Ordinal)
    {
        "glovo",
        "uber",
        "pyszne"
    };

    public DeliveryOrderAdapterResult Map(MockDeliveryOrderWebhook request)
    {
        var errors = new List<(string Key, string Error)>();
        var platform = request.Platform?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(platform))
        {
            errors.Add(("platform", "Platform is required."));
        }
        else if (!SupportedPlatforms.Contains(platform))
        {
            errors.Add(("platform", $"Platform '{request.Platform}' is not supported."));
        }

        if (!DeliveryStatusMapper.IsOrderCreationStatus(request.ExternalStatus))
        {
            errors.Add(("externalStatus", $"External status '{request.ExternalStatus}' is not supported by mock ingestion."));
        }

        if (errors.Count > 0)
        {
            return new DeliveryOrderAdapterResult(null, errors);
        }

        return new DeliveryOrderAdapterResult(
            new CreateOrderHandler.Command(
                request.BrandId,
                request.ExternalOrderId,
                $"mock-delivery:{platform}",
                request.Customer,
                request.Items),
            Array.Empty<(string Key, string Error)>());
    }
}
