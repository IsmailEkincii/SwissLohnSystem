using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class mig13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "ChurchMember",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HolidayEligible",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HolidayRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OvertimeRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PensumPercent",
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

            migrationBuilder.DropColumn(
                name: "WithholdingTaxCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkedHours",
                table: "Employees");

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeHours",
                table: "WorkDays",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursWorked",
                table: "WorkDays",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "DayType",
                table: "WorkDays",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Bonus",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraAllowance",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeduction",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnpaidDeduction",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Canton",
                table: "Employees",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayType",
                table: "WorkDays");

            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ExtraAllowance",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "OtherDeduction",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "UnpaidDeduction",
                table: "Lohns");

            migrationBuilder.AlterColumn<decimal>(
                name: "OvertimeHours",
                table: "WorkDays",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursWorked",
                table: "WorkDays",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Canton",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyAHV",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyALV",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBU",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBVG",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyFAK",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyNBU",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyQST",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayRate",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OvertimeHours",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeRate",
                table: "Employees",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PensumPercent",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitType",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "WithholdingTaxCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkedHours",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
