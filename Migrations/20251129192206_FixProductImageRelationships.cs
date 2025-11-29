using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_storefront_server.Migrations
{
    /// <inheritdoc />
    public partial class FixProductImageRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageUploads_Products_ProductId",
                table: "ImageUploads");

            migrationBuilder.DropIndex(
                name: "IX_ImageUploads_ProductId",
                table: "ImageUploads");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ImageUploads");

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImageUploadId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => new { x.ProductId, x.ImageUploadId });
                    table.ForeignKey(
                        name: "FK_ProductImages_ImageUploads_ImageUploadId",
                        column: x => x.ImageUploadId,
                        principalTable: "ImageUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ImageUploadId",
                table: "ProductImages",
                column: "ImageUploadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "ImageUploads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageUploads_ProductId",
                table: "ImageUploads",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageUploads_Products_ProductId",
                table: "ImageUploads",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
