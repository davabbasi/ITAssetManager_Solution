using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetInventoryRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseIssueId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WarehouseId",
                table: "Assets",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_WarehouseIssueId",
                table: "Assets",
                column: "WarehouseIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_WarehouseIssues_WarehouseIssueId",
                table: "Assets",
                column: "WarehouseIssueId",
                principalTable: "WarehouseIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Warehouses_WarehouseId",
                table: "Assets",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_WarehouseIssues_WarehouseIssueId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Warehouses_WarehouseId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_WarehouseId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_WarehouseIssueId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "WarehouseIssueId",
                table: "Assets");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
