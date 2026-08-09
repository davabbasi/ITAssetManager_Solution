using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addRelationToIssueItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WarehouseIssueItems_ProductId",
                table: "WarehouseIssueItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseIssueItems_Products_ProductId",
                table: "WarehouseIssueItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssueItems_Products_ProductId",
                table: "WarehouseIssueItems");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseIssueItems_ProductId",
                table: "WarehouseIssueItems");
        }
    }
}
