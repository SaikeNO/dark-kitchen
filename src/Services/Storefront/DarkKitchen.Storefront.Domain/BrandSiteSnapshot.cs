namespace DarkKitchen.Storefront.Domain;

public sealed class BrandSiteSnapshot
{
    private BrandSiteSnapshot()
    {
    }

    public Guid BrandId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public List<string> Domains { get; private set; } = [];
    public string? HeroTitle { get; private set; }
    public string? HeroSubtitle { get; private set; }
    public string PrimaryColor { get; private set; } = "#dc2626";
    public string AccentColor { get; private set; } = "#ca8a04";
    public string BackgroundColor { get; private set; } = "#fef2f2";
    public string TextColor { get; private set; } = "#450a0a";
    public bool IsActive { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static BrandSiteSnapshot Create(
        Guid brandId,
        string name,
        string? description,
        string? logoUrl,
        IReadOnlyList<string> domains,
        string? heroTitle,
        string? heroSubtitle,
        string primaryColor,
        string accentColor,
        string backgroundColor,
        string textColor,
        bool isActive,
        DateTimeOffset now)
    {
        var snapshot = new BrandSiteSnapshot { BrandId = brandId };
        snapshot.Update(name, description, logoUrl, domains, heroTitle, heroSubtitle, primaryColor, accentColor, backgroundColor, textColor, isActive, now);
        return snapshot;
    }

    public void Update(
        string name,
        string? description,
        string? logoUrl,
        IReadOnlyList<string> domains,
        string? heroTitle,
        string? heroSubtitle,
        string primaryColor,
        string accentColor,
        string backgroundColor,
        string textColor,
        bool isActive,
        DateTimeOffset now)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Domains = domains.Select(domain => domain.Trim().ToLowerInvariant()).Where(domain => domain.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        HeroTitle = heroTitle;
        HeroSubtitle = heroSubtitle;
        PrimaryColor = primaryColor;
        AccentColor = accentColor;
        BackgroundColor = backgroundColor;
        TextColor = textColor;
        IsActive = isActive;
        UpdatedAt = now;
    }
}
