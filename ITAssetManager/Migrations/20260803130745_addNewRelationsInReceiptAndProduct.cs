using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addNewRelationsInReceiptAndProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_WarehouseId",
                table: "WarehouseReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptItems_ProductId",
                table: "WarehouseReceiptItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseReceiptItems_Products_ProductId",
                table: "WarehouseReceiptItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseReceipts_Warehouses_WarehouseId",
                table: "WarehouseReceipts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseReceiptItems_Products_ProductId",
                table: "WarehouseReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseReceipts_Warehouses_WarehouseId",
                table: "WarehouseReceipts");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseReceipts_WarehouseId",
                table: "WarehouseReceipts");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseReceiptItems_ProductId",
                table: "WarehouseReceiptItems");
        }
    }
}
