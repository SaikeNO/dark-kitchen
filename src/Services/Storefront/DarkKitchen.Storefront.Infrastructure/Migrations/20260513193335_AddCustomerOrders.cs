using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkKitchen.Storefront.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_orders",
                schema: "storefront",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PickupCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_orders", x => x.OrderId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_orders_BrandId",
                schema: "storefront",
                table: "customer_orders",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_orders_CreatedAt",
                schema: "storefront",
                table: "customer_orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_customer_orders_UserId",
                schema: "storefront",
                table: "customer_orders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_orders",
                schema: "storefront");
        }
    }
}
