using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DarkKitchen.OrderManagement.Infrastructure.Persistence;

public sealed class OrderManagementDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrderManagementDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__order-db")
            ?? "Host=localhost;Port=5432;Database=darkkitchen_orders;Username=postgres;Password=postgres";

        options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(OrderManagementDbContextFactory).Assembly.FullName));

        return new OrderManagementDbContext(options.Options);
    }
}
