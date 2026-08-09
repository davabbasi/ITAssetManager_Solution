using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class changeWarehouseKeeper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeeperEmployeeFullName",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "KeeperEmployeeId",
                table: "Warehouses");

            migrationBuilder.AddColumn<int>(
                name: "KeeperId",
                table: "Warehouses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseKeeperId",
                table: "WarehouseReceipts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_KeeperId",
                table: "Warehouses",
                column: "KeeperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_WarehouseKeepers_KeeperId",
                table: "Warehouses",
                column: "KeeperId",
                principalTable: "WarehouseKeepers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_WarehouseKeepers_KeeperId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_KeeperId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "Warehouses");

            migrationBuilder.AddColumn<string>(
                name: "KeeperEmployeeFullName",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KeeperEmployeeId",
                table: "Warehouses",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseKeeperId",
                table: "WarehouseReceipts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
