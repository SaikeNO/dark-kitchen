using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkKitchen.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "recipe_snapshots",
                schema: "inventory",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_snapshots", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FailureReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "warehouse_items",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OnHandQuantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    MinSafetyLevel = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recipe_snapshot_items",
                schema: "inventory",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_snapshot_items", x => new { x.ProductId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_recipe_snapshot_items_recipe_snapshots_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "inventory",
                        principalTable: "recipe_snapshots",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_logs",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    OnHandAfter = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    ReservedAfter = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_logs_warehouse_items_WarehouseItemId",
                        column: x => x.WarehouseItemId,
                        principalSchema: "inventory",
                        principalTable: "warehouse_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservation_lines",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservation_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_reservation_lines_stock_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalSchema: "inventory",
                        principalTable: "stock_reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_reservation_lines_warehouse_items_WarehouseItemId",
                        column: x => x.WarehouseItemId,
                        principalSchema: "inventory",
                        principalTable: "warehouse_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_logs_CreatedAt",
                schema: "inventory",
                table: "inventory_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_logs_OrderId",
                schema: "inventory",
                table: "inventory_logs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_logs_WarehouseItemId",
                schema: "inventory",
                table: "inventory_logs",
                column: "WarehouseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservation_lines_ReservationId_WarehouseItemId",
                schema: "inventory",
                table: "stock_reservation_lines",
                columns: new[] { "ReservationId", "WarehouseItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservation_lines_WarehouseItemId",
                schema: "inventory",
                table: "stock_reservation_lines",
                column: "WarehouseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_OrderId",
                schema: "inventory",
                table: "stock_reservations",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_items_Name",
                schema: "inventory",
                table: "warehouse_items",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_logs",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "recipe_snapshot_items",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_reservation_lines",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "recipe_snapshots",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "stock_reservations",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "warehouse_items",
                schema: "inventory");
        }
    }
}
