using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DineNDash.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuCategoriesAndImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RestaurantId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuCategories_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "MenuCategoryId",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            // Keep existing menu items by turning each restaurant's distinct Category text into a category row
            migrationBuilder.Sql("""
                INSERT INTO "MenuCategories" ("Name", "RestaurantId", "CreatedAt")
                SELECT DISTINCT COALESCE(NULLIF(TRIM("Category"), ''), 'Menu'), "RestaurantId", NOW()
                FROM "MenuItems";

                UPDATE "MenuItems" AS item
                SET "MenuCategoryId" = category."Id"
                FROM "MenuCategories" AS category
                WHERE category."RestaurantId" = item."RestaurantId"
                  AND category."Name" = COALESCE(NULLIF(TRIM(item."Category"), ''), 'Menu');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "MenuCategoryId",
                table: "MenuItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "MenuItems");

            // Image links cannot be turned into uploaded files, so owners upload their images again
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "PhotoUrls",
                table: "Restaurants");

            migrationBuilder.AddColumn<int>(
                name: "ProfileImageId",
                table: "Restaurants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuCategoryId",
                table: "MenuItems",
                column: "MenuCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_ProfileImageId",
                table: "Restaurants",
                column: "ProfileImageId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ImageId",
                table: "MenuItems",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_FileName",
                table: "Images",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_RestaurantId_Name",
                table: "MenuCategories",
                columns: new[] { "RestaurantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Images_ImageId",
                table: "MenuItems",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems",
                column: "MenuCategoryId",
                principalTable: "MenuCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Images_ProfileImageId",
                table: "Restaurants",
                column: "ProfileImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Images_ImageId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Images_ProfileImageId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_ProfileImageId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ImageId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ProfileImageId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "MenuItems");

            migrationBuilder.AddColumn<string[]>(
                name: "PhotoUrls",
                table: "Restaurants",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "MenuItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "MenuItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Copy each item's category name and restaurant back onto the item before the categories are dropped
            migrationBuilder.Sql("""
                UPDATE "MenuItems" AS item
                SET "Category" = category."Name", "RestaurantId" = category."RestaurantId"
                FROM "MenuCategories" AS category
                WHERE category."Id" = item."MenuCategoryId";
                """);

            migrationBuilder.DropColumn(
                name: "MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "MenuCategories");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
