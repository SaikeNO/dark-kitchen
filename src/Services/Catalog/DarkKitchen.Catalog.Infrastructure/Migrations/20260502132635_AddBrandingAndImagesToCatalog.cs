using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkKitchen.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandingAndImagesToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "catalog",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                schema: "catalog",
                table: "brands",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "#ca8a04");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                schema: "catalog",
                table: "brands",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "#fef2f2");

            migrationBuilder.AddColumn<List<string>>(
                name: "Domains",
                schema: "catalog",
                table: "brands",
                type: "text[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::text[]");

            migrationBuilder.AddColumn<string>(
                name: "HeroSubtitle",
                schema: "catalog",
                table: "brands",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                schema: "catalog",
                table: "brands",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                schema: "catalog",
                table: "brands",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "#dc2626");

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                schema: "catalog",
                table: "brands",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "#450a0a");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "AccentColor",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "Domains",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "HeroSubtitle",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                schema: "catalog",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "TextColor",
                schema: "catalog",
                table: "brands");
        }
    }
}
