using Microsoft.AspNetCore.Identity;

namespace DarkKitchen.Storefront.Features.Application;

public sealed class StorefrontUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
}
