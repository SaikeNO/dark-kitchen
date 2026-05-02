using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.Storefront.Infrastructure.Persistence;

public sealed class StorefrontDbContextFactory : IDesignTimeDbContextFactory<StorefrontDbContext>
{
    public StorefrontDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StorefrontDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__storefront-db")
            ?? "Host=localhost;Port=5432;Database=darkkitchen_storefront;Username=postgres;Password=postgres";

        options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(StorefrontDbContextFactory).Assembly.FullName));

        return new StorefrontDbContext(options.Options);
    }
}
