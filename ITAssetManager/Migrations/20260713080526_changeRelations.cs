using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class changeRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySpecifications_Categories_CategoryId",
                table: "CategorySpecifications");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySpecifications_Categories_CategoryId",
                table: "CategorySpecifications",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySpecifications_Categories_CategoryId",
                table: "CategorySpecifications");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySpecifications_Categories_CategoryId",
                table: "CategorySpecifications",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
