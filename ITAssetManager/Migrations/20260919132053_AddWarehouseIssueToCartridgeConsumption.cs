using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseIssueToCartridgeConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseIssueId",
                table: "CartridgeConsumptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartridgeConsumptions_WarehouseIssueId",
                table: "CartridgeConsumptions",
                column: "WarehouseIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumptions_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumptions",
                column: "WarehouseIssueId",
                principalTable: "WarehouseIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumptions_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_CartridgeConsumptions_WarehouseIssueId",
                table: "CartridgeConsumptions");

            migrationBuilder.DropColumn(
                name: "WarehouseIssueId",
                table: "CartridgeConsumptions");
        }
    }
}
