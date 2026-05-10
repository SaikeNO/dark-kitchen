using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkKitchen.Kds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialKdsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kds");

            migrationBuilder.CreateTable(
                name: "kitchen_stations",
                schema: "kds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DisplayColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kitchen_tickets",
                schema: "kds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceChannel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_station_route_snapshots",
                schema: "kds",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_station_route_snapshots", x => new { x.BrandId, x.ProductId });
                });

            migrationBuilder.CreateTable(
                name: "kitchen_tasks",
                schema: "kds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kitchen_tasks_kitchen_tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "kds",
                        principalTable: "kitchen_tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_stations_Code",
                schema: "kds",
                table: "kitchen_stations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_stations_IsActive",
                schema: "kds",
                table: "kitchen_stations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_tasks_OrderItemId",
                schema: "kds",
                table: "kitchen_tasks",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_tasks_StationId_Status_CreatedAt",
                schema: "kds",
                table: "kitchen_tasks",
                columns: new[] { "StationId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_tasks_TicketId",
                schema: "kds",
                table: "kitchen_tasks",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_tickets_BrandId",
                schema: "kds",
                table: "kitchen_tickets",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_kitchen_tickets_OrderId",
                schema: "kds",
                table: "kitchen_tickets",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_station_route_snapshots_StationId",
                schema: "kds",
                table: "product_station_route_snapshots",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kitchen_stations",
                schema: "kds");

            migrationBuilder.DropTable(
                name: "kitchen_tasks",
                schema: "kds");

            migrationBuilder.DropTable(
                name: "product_station_route_snapshots",
                schema: "kds");

            migrationBuilder.DropTable(
                name: "kitchen_tickets",
                schema: "kds");
        }
    }
}
