using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class mig12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyHours",
                table: "Lohns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyOvertimeHours",
                table: "Lohns",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "MonthlyHours",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "MonthlyOvertimeHours",
                table: "Lohns");
        }
    }
}
