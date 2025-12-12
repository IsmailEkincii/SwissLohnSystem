using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerSnapshotToLohn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EmployerAhvIvEo",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerAlv",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerBu",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerBvg",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerFak",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployerAhvIvEo",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerAlv",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerBu",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerBvg",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerFak",
                table: "Lohns");
        }
    }
}
