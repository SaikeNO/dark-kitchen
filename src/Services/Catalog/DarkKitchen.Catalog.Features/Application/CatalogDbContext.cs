using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Catalog.Features.Application;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : IdentityDbContext<CatalogUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<ProductStationRoute> ProductStationRoutes => Set<ProductStationRoute>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.MapWolverineEnvelopeStorage();
        ConfigureIdentity(builder);
        ConfigureCatalog(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<CatalogUser>().ToTable("users", "identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
    }

    private static void ConfigureCatalog(ModelBuilder builder)
    {
        builder.Entity<Brand>(entity =>
        {
            entity.ToTable("brands", "catalog");
            entity.HasKey(brand => brand.Id);
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
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("categories", "catalog");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(120).IsRequired();
            entity.HasOne(category => category.Brand)
                .WithMany(brand => brand.Categories)
                .HasForeignKey(category => category.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(category => new { category.BrandId, category.Name }).IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products", "catalog");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(160).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(1000);
            entity.Property(product => product.ImageUrl).HasMaxLength(500);
            entity.Property(product => product.Price).HasPrecision(10, 2);
            entity.Property(product => product.Currency).HasMaxLength(3).IsRequired();
            entity.HasOne(product => product.Brand)
                .WithMany(brand => brand.Products)
                .HasForeignKey(product => product.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(product => new { product.BrandId, product.Name }).IsUnique();
        });

        builder.Entity<Ingredient>(entity =>
        {
            entity.ToTable("ingredients", "catalog");
            entity.HasKey(ingredient => ingredient.Id);
            entity.Property(ingredient => ingredient.Name).HasMaxLength(120).IsRequired();
            entity.Property(ingredient => ingredient.Unit).HasMaxLength(16).IsRequired();
            entity.HasIndex(ingredient => ingredient.Name).IsUnique();
        });

        builder.Entity<Recipe>(entity =>
        {
            entity.ToTable("recipes", "catalog");
            entity.HasKey(recipe => recipe.ProductId);
            entity.HasOne(recipe => recipe.Product)
                .WithOne(product => product.Recipe)
                .HasForeignKey<Recipe>(recipe => recipe.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RecipeItem>(entity =>
        {
            entity.ToTable("recipe_items", "catalog");
            entity.HasKey(item => new { item.ProductId, item.IngredientId });
            entity.Property(item => item.Quantity).HasPrecision(10, 3);
            entity.HasOne(item => item.Recipe)
                .WithMany(recipe => recipe.Items)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Ingredient)
                .WithMany(ingredient => ingredient.RecipeItems)
                .HasForeignKey(item => item.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Station>(entity =>
        {
            entity.ToTable("stations", "catalog");
            entity.HasKey(station => station.Id);
            entity.Property(station => station.Code).HasMaxLength(32).IsRequired();
            entity.Property(station => station.Name).HasMaxLength(120).IsRequired();
            entity.Property(station => station.DisplayColor).HasMaxLength(16).IsRequired();
            entity.HasIndex(station => station.Code).IsUnique();
        });

        builder.Entity<ProductStationRoute>(entity =>
        {
            entity.ToTable("product_station_routes", "catalog");
            entity.HasKey(route => route.ProductId);
            entity.HasOne(route => route.Product)
                .WithOne(product => product.StationRoute)
                .HasForeignKey<ProductStationRoute>(route => route.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(route => route.Station)
                .WithMany(station => station.Routes)
                .HasForeignKey(route => route.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
