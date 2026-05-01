using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.Catalog.Api;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__catalog-db")
            ?? "Host=localhost;Port=5432;Database=darkkitchen_catalog;Username=postgres;Password=postgres";

        options.UseNpgsql(connectionString);

        return new CatalogDbContext(options.Options);
    }
}
