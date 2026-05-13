using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkKitchen.Packing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPackingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "packing");

            migrationBuilder.CreateTable(
                name: "packing_manifests",
                schema: "packing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceChannel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadyForPackingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_packing_manifests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pending_prepared_items",
                schema: "packing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_prepared_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manifest_items",
                schema: "packing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsReady = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_manifest_items_packing_manifests_ManifestId",
                        column: x => x.ManifestId,
                        principalSchema: "packing",
                        principalTable: "packing_manifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manifest_items_ManifestId",
                schema: "packing",
                table: "manifest_items",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_items_ManifestId_IsReady",
                schema: "packing",
                table: "manifest_items",
                columns: new[] { "ManifestId", "IsReady" });

            migrationBuilder.CreateIndex(
                name: "IX_manifest_items_OrderItemId",
                schema: "packing",
                table: "manifest_items",
                column: "OrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_packing_manifests_BrandId",
                schema: "packing",
                table: "packing_manifests",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_packing_manifests_OrderId",
                schema: "packing",
                table: "packing_manifests",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_packing_manifests_Status_CreatedAt",
                schema: "packing",
                table: "packing_manifests",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_pending_prepared_items_OrderId",
                schema: "packing",
                table: "pending_prepared_items",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_pending_prepared_items_OrderItemId",
                schema: "packing",
                table: "pending_prepared_items",
                column: "OrderItemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manifest_items",
                schema: "packing");

            migrationBuilder.DropTable(
                name: "pending_prepared_items",
                schema: "packing");

            migrationBuilder.DropTable(
                name: "packing_manifests",
                schema: "packing");
        }
    }
}
