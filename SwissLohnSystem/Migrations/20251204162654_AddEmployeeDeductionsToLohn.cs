using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeDeductionsToLohn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeAhvIvEo",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeAlv",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeBvg",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeNbu",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeQst",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeAhvIvEo",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeAlv",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeBvg",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeNbu",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeQst",
                table: "Lohns");
        }
    }
}
