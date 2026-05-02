namespace DarkKitchen.Storefront.Features.Features.Auth;

public sealed record CustomerSessionResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    string? Phone);

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string? DisplayName,
    string? Phone);

public sealed record LoginCustomerRequest(
    string Email,
    string Password);
