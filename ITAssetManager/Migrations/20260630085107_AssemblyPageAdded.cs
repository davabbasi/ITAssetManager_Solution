using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class AssemblyPageAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssemblyNumber",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssembled",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AssemblyComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PcAssetId = table.Column<int>(type: "int", nullable: false),
                    ComponentAssetId = table.Column<int>(type: "int", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InstalledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyComponents_Assets_ComponentAssetId",
                        column: x => x.ComponentAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssemblyComponents_Assets_PcAssetId",
                        column: x => x.PcAssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_ComponentAssetId",
                table: "AssemblyComponents",
                column: "ComponentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_PcAssetId",
                table: "AssemblyComponents",
                column: "PcAssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssemblyComponents");

            migrationBuilder.DropColumn(
                name: "AssemblyNumber",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsAssembled",
                table: "Assets");
        }
    }
}
