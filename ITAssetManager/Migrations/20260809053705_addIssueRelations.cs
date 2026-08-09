using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addIssueRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ToWarehouseId",
                table: "WarehouseIssues",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "WarehouseIssues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseIssues_FromWarehouseId",
                table: "WarehouseIssues",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseIssues_ToWarehouseId",
                table: "WarehouseIssues",
                column: "ToWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseIssues_Warehouses_FromWarehouseId",
                table: "WarehouseIssues",
                column: "FromWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseIssues_Warehouses_ToWarehouseId",
                table: "WarehouseIssues",
                column: "ToWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_FromWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_ToWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseIssues_FromWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseIssues_ToWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.AlterColumn<int>(
                name: "ToWarehouseId",
                table: "WarehouseIssues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "WarehouseIssues",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
