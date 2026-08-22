using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetToInventoryTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_AssetId",
                table: "InventoryTransactions",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Assets_AssetId",
                table: "InventoryTransactions",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Assets_AssetId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_AssetId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "InventoryTransactions");
        }
    }
}
