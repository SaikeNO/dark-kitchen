using DarkKitchen.Packing.Features.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.Packing.Infrastructure.Persistence;

public sealed class PackingDbContextFactory : IDesignTimeDbContextFactory<PackingDbContext>
{
    public PackingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PackingDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=darkkitchen_packing;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsAssembly(typeof(PackingDbContextFactory).Assembly.FullName))
            .Options;

        return new PackingDbContext(options);
    }
}
