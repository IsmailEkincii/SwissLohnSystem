using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class EmployeePayrollFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeRate",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HolidayRate",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BruttoSalary",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
     name: "ApplyAHV",
     table: "Employees",
     type: "bit",
     nullable: false,
     defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyALV",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBU",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBVG",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyFAK",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyNBU",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyQST",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Canton",
                table: "Employees",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "ZH");

            migrationBuilder.AddColumn<bool>(
                name: "ChurchMember",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HolidayEligible",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PermitType",
                table: "Employees",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "B");

            migrationBuilder.AddColumn<bool>(
                name: "ThirteenthEligible",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ThirteenthProrated",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyHours",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyAHV",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyALV",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyBU",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyBVG",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyFAK",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyNBU",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApplyQST",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Canton",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ChurchMember",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HolidayEligible",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermitType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ThirteenthEligible",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ThirteenthProrated",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "Employees");

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeRate",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyRate",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HolidayRate",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BruttoSalary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
