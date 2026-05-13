using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace DarkKitchen.Packing.Features.Application;

public sealed class PackingDbContext(DbContextOptions<PackingDbContext> options) : DbContext(options)
{
    public DbSet<PackingManifest> PackingManifests => Set<PackingManifest>();
    public DbSet<ManifestItem> ManifestItems => Set<ManifestItem>();
    public DbSet<PendingPreparedItem> PendingPreparedItems => Set<PendingPreparedItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.MapWolverineEnvelopeStorage();
        ConfigurePacking(builder);
    }

    private static void ConfigurePacking(ModelBuilder builder)
    {
        builder.Entity<PackingManifest>(entity =>
        {
            entity.ToTable("packing_manifests", "packing");
            entity.HasKey(manifest => manifest.Id);
            entity.Property(manifest => manifest.SourceChannel).HasMaxLength(64).IsRequired();
            entity.Property(manifest => manifest.Status).HasConversion<int>();
            entity.Ignore(manifest => manifest.TotalItemsCount);
            entity.Ignore(manifest => manifest.ReadyItemsCount);
            entity.HasIndex(manifest => manifest.OrderId).IsUnique();
            entity.HasIndex(manifest => new { manifest.Status, manifest.CreatedAt });
            entity.HasIndex(manifest => manifest.BrandId);
            entity.HasMany(manifest => manifest.Items)
                .WithOne(item => item.Manifest)
                .HasForeignKey(item => item.ManifestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(manifest => manifest.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<ManifestItem>(entity =>
        {
            entity.ToTable("manifest_items", "packing");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ItemName).HasMaxLength(200).IsRequired();
            entity.HasIndex(item => item.OrderItemId).IsUnique();
            entity.HasIndex(item => item.ManifestId);
            entity.HasIndex(item => new { item.ManifestId, item.IsReady });
        });

        builder.Entity<PendingPreparedItem>(entity =>
        {
            entity.ToTable("pending_prepared_items", "packing");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.StationCode).HasMaxLength(64).IsRequired();
            entity.Property(item => item.BrandId).HasMaxLength(64).IsRequired();
            entity.HasIndex(item => item.OrderItemId).IsUnique();
            entity.HasIndex(item => item.OrderId);
        });
    }
}
