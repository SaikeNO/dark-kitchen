using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Storefront.Features.Application;

public sealed class StorefrontDbContext(DbContextOptions<StorefrontDbContext> options)
    : IdentityDbContext<StorefrontUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BrandSiteSnapshot> BrandSites => Set<BrandSiteSnapshot>();
    public DbSet<MenuCategorySnapshot> MenuCategories => Set<MenuCategorySnapshot>();
    public DbSet<MenuItemSnapshot> MenuItems => Set<MenuItemSnapshot>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.MapWolverineEnvelopeStorage();
        ConfigureIdentity(builder);
        ConfigureStorefront(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<StorefrontUser>().ToTable("users", "identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
    }

    private static void ConfigureStorefront(ModelBuilder builder)
    {
        builder.Entity<BrandSiteSnapshot>(entity =>
        {
            entity.ToTable("brand_sites", "storefront");
            entity.HasKey(brand => brand.BrandId);
            entity.Property(brand => brand.Name).HasMaxLength(120).IsRequired();
            entity.Property(brand => brand.Description).HasMaxLength(500);
            entity.Property(brand => brand.LogoUrl).HasMaxLength(500);
            entity.Property(brand => brand.Domains).HasColumnType("text[]");
            entity.Property(brand => brand.HeroTitle).HasMaxLength(160);
            entity.Property(brand => brand.HeroSubtitle).HasMaxLength(500);
            entity.Property(brand => brand.PrimaryColor).HasMaxLength(16).IsRequired();
            entity.Property(brand => brand.AccentColor).HasMaxLength(16).IsRequired();
            entity.Property(brand => brand.BackgroundColor).HasMaxLength(16).IsRequired();
            entity.Property(brand => brand.TextColor).HasMaxLength(16).IsRequired();
            entity.HasIndex(brand => brand.IsActive);
        });

        builder.Entity<MenuCategorySnapshot>(entity =>
        {
            entity.ToTable("menu_categories", "storefront");
            entity.HasKey(category => category.CategoryId);
            entity.Property(category => category.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(category => new { category.BrandId, category.IsActive });
        });

        builder.Entity<MenuItemSnapshot>(entity =>
        {
            entity.ToTable("menu_items", "storefront");
            entity.HasKey(item => item.MenuItemId);
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.ImageUrl).HasMaxLength(500);
            entity.Property(item => item.Price).HasPrecision(10, 2);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.HasIndex(item => new { item.BrandId, item.IsActive });
            entity.HasIndex(item => item.CategoryId);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts", "storefront");
            entity.HasKey(cart => cart.Id);
            entity.Ignore(cart => cart.TotalPrice);
            entity.Ignore(cart => cart.Currency);
            entity.HasMany(cart => cart.Items)
                .WithOne(item => item.Cart)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(cart => cart.BrandId);
            entity.HasIndex(cart => cart.UserId);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items", "storefront");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(160).IsRequired();
            entity.Property(item => item.ImageUrl).HasMaxLength(500);
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Ignore(item => item.LineTotal);
            entity.HasIndex(item => new { item.CartId, item.MenuItemId }).IsUnique();
        });

        builder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("payment_transactions", "storefront");
            entity.HasKey(payment => payment.Id);
            entity.Property(payment => payment.ExternalTransactionId).HasMaxLength(80).IsRequired();
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(payment => payment.Amount).HasPrecision(10, 2);
            entity.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
            entity.Property(payment => payment.FailureReason).HasMaxLength(200);
            entity.HasIndex(payment => payment.BrandId);
            entity.HasIndex(payment => payment.CartId);
        });
    }
}
