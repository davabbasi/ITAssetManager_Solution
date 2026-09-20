using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addBatteryProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.RenameTable(
                name: "CartridgeConsumptions",
                newName: "CartridgeConsumption");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartridgeConsumption",
                table: "CartridgeConsumption",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BatteryConsumption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UPS_Id = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ConsumptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarehouseIssueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatteryConsumption_Assets_UPS_Id",
                        column: x => x.UPS_Id,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatteryConsumption_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatteryConsumption_WarehouseIssues_WarehouseIssueId",
                        column: x => x.WarehouseIssueId,
                        principalTable: "WarehouseIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatteryConsumption_ProductId",
                table: "BatteryConsumption",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryConsumption_UPS_Id",
                table: "BatteryConsumption",
                column: "UPS_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryConsumption_WarehouseIssueId",
                table: "BatteryConsumption",
                column: "WarehouseIssueId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_Assets_PrinterId",
                table: "CartridgeConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_Products_ProductId",
                table: "CartridgeConsumption");

            migrationBuilder.DropForeignKey(
                name: "FK_CartridgeConsumption_WarehouseIssues_WarehouseIssueId",
                table: "CartridgeConsumption");

            migrationBuilder.DropTable(
                name: "BatteryConsumption");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartridgeConsumption",
                table: "CartridgeConsumption");

            migrationBuilder.RenameTable(
                name: "CartridgeConsumption",
                newName: "CartridgeConsumptions");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartridgeConsumptions",
                table: "CartridgeConsumptions",
                column: "Id");

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
    }
}
