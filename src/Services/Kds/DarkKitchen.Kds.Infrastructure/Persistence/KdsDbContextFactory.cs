using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.Kds.Infrastructure.Persistence;

public sealed class KdsDbContextFactory : IDesignTimeDbContextFactory<KdsDbContext>
{
    public KdsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<KdsDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__kds-db")
            ?? "Host=localhost;Port=5432;Database=darkkitchen_kds;Username=postgres;Password=postgres";

        options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(KdsDbContextFactory).Assembly.FullName));

        return new KdsDbContext(options.Options);
    }
}
