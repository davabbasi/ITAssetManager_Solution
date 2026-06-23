using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManager.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocalDepartmentAndEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Departments_FromDepartmentId",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Departments_ToDepartmentId",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Employees_FromEmployeeId",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetAssignments_Employees_ToEmployeeId",
                table: "AssetAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Departments_DepartmentId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Employees_EmployeeId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_EmployeeId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_FromDepartmentId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_FromEmployeeId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_ToDepartmentId",
                table: "AssetAssignments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAssignments_ToEmployeeId",
                table: "AssetAssignments");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromDepartmentName",
                table: "AssetAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromEmployeeName",
                table: "AssetAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToDepartmentName",
                table: "AssetAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToEmployeeName",
                table: "AssetAssignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "FromDepartmentName",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "FromEmployeeName",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "ToDepartmentName",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "ToEmployeeName",
                table: "AssetAssignments");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DepartmentId",
                table: "Assets",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_EmployeeId",
                table: "Assets",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_FromDepartmentId",
                table: "AssetAssignments",
                column: "FromDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_FromEmployeeId",
                table: "AssetAssignments",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ToDepartmentId",
                table: "AssetAssignments",
                column: "ToDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAssignments_ToEmployeeId",
                table: "AssetAssignments",
                column: "ToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Departments_FromDepartmentId",
                table: "AssetAssignments",
                column: "FromDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Departments_ToDepartmentId",
                table: "AssetAssignments",
                column: "ToDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Employees_FromEmployeeId",
                table: "AssetAssignments",
                column: "FromEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAssignments_Employees_ToEmployeeId",
                table: "AssetAssignments",
                column: "ToEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Departments_DepartmentId",
                table: "Assets",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Employees_EmployeeId",
                table: "Assets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
