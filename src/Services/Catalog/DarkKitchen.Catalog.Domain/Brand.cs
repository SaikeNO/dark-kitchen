namespace DarkKitchen.Catalog.Domain;

public sealed class Brand
{
    public const string DefaultPrimaryColor = "#dc2626";
    public const string DefaultAccentColor = "#ca8a04";
    public const string DefaultBackgroundColor = "#fef2f2";
    public const string DefaultTextColor = "#450a0a";

    private Brand()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public List<string> Domains { get; private set; } = [];
    public string? HeroTitle { get; private set; }
    public string? HeroSubtitle { get; private set; }
    public string PrimaryColor { get; private set; } = DefaultPrimaryColor;
    public string AccentColor { get; private set; } = DefaultAccentColor;
    public string BackgroundColor { get; private set; } = DefaultBackgroundColor;
    public string TextColor { get; private set; } = DefaultTextColor;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<Category> Categories { get; private set; } = [];
    public List<Product> Products { get; private set; } = [];

    public static Brand Create(
        string name,
        string? description,
        string? logoUrl,
        bool isActive,
        DateTimeOffset now,
        Guid? id = null,
        IReadOnlyList<string>? domains = null,
        string? heroTitle = null,
        string? heroSubtitle = null,
        string? primaryColor = null,
        string? accentColor = null,
        string? backgroundColor = null,
        string? textColor = null)
    {
        return new Brand
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            LogoUrl = logoUrl,
            Domains = NormalizeDomains(domains),
            HeroTitle = heroTitle,
            HeroSubtitle = heroSubtitle,
            PrimaryColor = NormalizeColor(primaryColor, DefaultPrimaryColor),
            AccentColor = NormalizeColor(accentColor, DefaultAccentColor),
            BackgroundColor = NormalizeColor(backgroundColor, DefaultBackgroundColor),
            TextColor = NormalizeColor(textColor, DefaultTextColor),
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string name,
        string? description,
        string? logoUrl,
        bool isActive,
        DateTimeOffset now,
        IReadOnlyList<string>? domains = null,
        string? heroTitle = null,
        string? heroSubtitle = null,
        string? primaryColor = null,
        string? accentColor = null,
        string? backgroundColor = null,
        string? textColor = null)
    {
        Name = name;
        Description = description;
        LogoUrl = logoUrl;
        Domains = NormalizeDomains(domains);
        HeroTitle = heroTitle;
        HeroSubtitle = heroSubtitle;
        PrimaryColor = NormalizeColor(primaryColor, DefaultPrimaryColor);
        AccentColor = NormalizeColor(accentColor, DefaultAccentColor);
        BackgroundColor = NormalizeColor(backgroundColor, DefaultBackgroundColor);
        TextColor = NormalizeColor(textColor, DefaultTextColor);
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAt = now;
    }

    private static List<string> NormalizeDomains(IReadOnlyList<string>? domains)
    {
        return domains is null
            ? []
            : domains
                .Select(domain => domain.Trim().ToLowerInvariant())
                .Where(domain => domain.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToList();
    }

    private static string NormalizeColor(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToLowerInvariant();
    }
}
