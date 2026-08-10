using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class modifyIssueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_FromWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_ToWarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.RenameColumn(
                name: "ToWarehouseId",
                table: "WarehouseIssues",
                newName: "WarehouseId1");

            migrationBuilder.RenameColumn(
                name: "FromWarehouseId",
                table: "WarehouseIssues",
                newName: "WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseIssues_ToWarehouseId",
                table: "WarehouseIssues",
                newName: "IX_WarehouseIssues_WarehouseId1");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseIssues_FromWarehouseId",
                table: "WarehouseIssues",
                newName: "IX_WarehouseIssues_WarehouseId");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "WarehouseIssues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "WarehouseIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseIssues_Warehouses_WarehouseId",
                table: "WarehouseIssues",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarehouseIssues_Warehouses_WarehouseId1",
                table: "WarehouseIssues",
                column: "WarehouseId1",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_WarehouseId",
                table: "WarehouseIssues");

            migrationBuilder.DropForeignKey(
                name: "FK_WarehouseIssues_Warehouses_WarehouseId1",
                table: "WarehouseIssues");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "WarehouseIssues");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "WarehouseIssues");

            migrationBuilder.RenameColumn(
                name: "WarehouseId1",
                table: "WarehouseIssues",
                newName: "ToWarehouseId");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "WarehouseIssues",
                newName: "FromWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseIssues_WarehouseId1",
                table: "WarehouseIssues",
                newName: "IX_WarehouseIssues_ToWarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_WarehouseIssues_WarehouseId",
                table: "WarehouseIssues",
                newName: "IX_WarehouseIssues_FromWarehouseId");

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
    }
}
