using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class addNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specs",
                table: "Assets");

            migrationBuilder.CreateTable(
                name: "SpecDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecDefinitions_AssetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssetCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpecValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecDefinitionId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecValues_SpecDefinitions_SpecDefinitionId",
                        column: x => x.SpecDefinitionId,
                        principalTable: "SpecDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetSpecValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    SpecDefinitionId = table.Column<int>(type: "int", nullable: false),
                    SpecValueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetSpecValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetSpecValues_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetSpecValues_SpecDefinitions_SpecDefinitionId",
                        column: x => x.SpecDefinitionId,
                        principalTable: "SpecDefinitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetSpecValues_SpecValues_SpecValueId",
                        column: x => x.SpecValueId,
                        principalTable: "SpecValues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecValues_AssetId",
                table: "AssetSpecValues",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecValues_SpecDefinitionId",
                table: "AssetSpecValues",
                column: "SpecDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecValues_SpecValueId",
                table: "AssetSpecValues",
                column: "SpecValueId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecDefinitions_CategoryId",
                table: "SpecDefinitions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecValues_SpecDefinitionId",
                table: "SpecValues",
                column: "SpecDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetSpecValues");

            migrationBuilder.DropTable(
                name: "SpecValues");

            migrationBuilder.DropTable(
                name: "SpecDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "Specs",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
