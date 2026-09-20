using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addBatteryProductEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatteryConsumption_Assets_UPS_Id",
                table: "BatteryConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_BatteryConsumption_Products_ProductId",
                table: "BatteryConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_BatteryConsumption_WarehouseIssues_WarehouseIssueId",
                table: "BatteryConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_Assets_PrinterId",
                table: "CartridgeConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_Products_ProductId",
                table: "CartridgeConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartridgeConsumption",
                table: "CartridgeConsumption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatteryConsumption",
                table: "BatteryConsumption");

            migrationBuilder.RenameTable(
                name: "CartridgeConsumption",
                newName: "CartridgeConsumptions");

            migrationBuilder.RenameTable(
                name: "BatteryConsumption",
                newName: "BatteryConsumptions");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumption_WarehouseIssueId",
                table: "CartridgeConsumptions",
                newName: "IX_CartridgeConsumptions_WarehouseIssueId");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumption_ProductId",
                table: "CartridgeConsumptions",
                newName: "IX_CartridgeConsumptions_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumption_PrinterId",
                table: "CartridgeConsumptions",
                newName: "IX_CartridgeConsumptions_PrinterId");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumption_WarehouseIssueId",
                table: "BatteryConsumptions",
                newName: "IX_BatteryConsumptions_WarehouseIssueId");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumption_UPS_Id",
                table: "BatteryConsumptions",
                newName: "IX_BatteryConsumptions_UPS_Id");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumption_ProductId",
                table: "BatteryConsumptions",
                newName: "IX_BatteryConsumptions_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartridgeConsumptions",
                table: "CartridgeConsumptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatteryConsumptions",
                table: "BatteryConsumptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumptions_Assets_UPS_Id",
                table: "BatteryConsumptions",
                column: "UPS_Id",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumptions_Products_ProductId",
                table: "BatteryConsumptions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumptions_WarehouseIssues_WarehouseIssueId",
                table: "BatteryConsumptions",
                column: "WarehouseIssueId",
                principalTable: "WarehouseIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumptions_Assets_PrinterId",
                table: "CartridgeConsumptions",
                column: "PrinterId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumptions_Products_ProductId",
                table: "CartridgeConsumptions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_BatteryConsumptions_Assets_UPS_Id",
                table: "BatteryConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_BatteryConsumptions_Products_ProductId",
                table: "BatteryConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_BatteryConsumptions_WarehouseIssues_WarehouseIssueId",
                table: "BatteryConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumptions_Assets_PrinterId",
                table: "CartridgeConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumptions_Products_ProductId",
                table: "CartridgeConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumptions_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartridgeConsumptions",
                table: "CartridgeConsumptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatteryConsumptions",
                table: "BatteryConsumptions");

            migrationBuilder.RenameTable(
                name: "CartridgeConsumptions",
                newName: "CartridgeConsumption");

            migrationBuilder.RenameTable(
                name: "BatteryConsumptions",
                newName: "BatteryConsumption");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumptions_WarehouseIssueId",
                table: "CartridgeConsumption",
                newName: "IX_CartridgeConsumption_WarehouseIssueId");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumptions_ProductId",
                table: "CartridgeConsumption",
                newName: "IX_CartridgeConsumption_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartridgeConsumptions_PrinterId",
                table: "CartridgeConsumption",
                newName: "IX_CartridgeConsumption_PrinterId");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumptions_WarehouseIssueId",
                table: "BatteryConsumption",
                newName: "IX_BatteryConsumption_WarehouseIssueId");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumptions_UPS_Id",
                table: "BatteryConsumption",
                newName: "IX_BatteryConsumption_UPS_Id");

            migrationBuilder.RenameIndex(
                name: "IX_BatteryConsumptions_ProductId",
                table: "BatteryConsumption",
                newName: "IX_BatteryConsumption_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartridgeConsumption",
                table: "CartridgeConsumption",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatteryConsumption",
                table: "BatteryConsumption",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumption_Assets_UPS_Id",
                table: "BatteryConsumption",
                column: "UPS_Id",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumption_Products_ProductId",
                table: "BatteryConsumption",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryConsumption_WarehouseIssues_WarehouseIssueId",
                table: "BatteryConsumption",
                column: "WarehouseIssueId",
                principalTable: "WarehouseIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumption_Assets_PrinterId",
                table: "CartridgeConsumption",
                column: "PrinterId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumption_Products_ProductId",
                table: "CartridgeConsumption",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartridgeConsumption_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumption",
                column: "WarehouseIssueId",
                principalTable: "WarehouseIssues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
