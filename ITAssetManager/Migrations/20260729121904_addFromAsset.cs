using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addFromAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromAssetId",
                table: "AssemblyComponents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_FromAssetId",
                table: "AssemblyComponents",
                column: "FromAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssemblyComponents_Assets_FromAssetId",
                table: "AssemblyComponents",
                column: "FromAssetId",
                principalTable: "Assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssemblyComponents_Assets_FromAssetId",
                table: "AssemblyComponents");

            migrationBuilder.DropIndex(
                name: "IX_AssemblyComponents_FromAssetId",
                table: "AssemblyComponents");

            migrationBuilder.DropColumn(
                name: "FromAssetId",
                table: "AssemblyComponents");
        }
    }
}
