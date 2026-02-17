using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class KTGFEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe drops (works even if index doesn't exist)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Settings_Name' AND object_id = OBJECT_ID(N'[dbo].[Settings]'))
    DROP INDEX [IX_Settings_Name] ON [dbo].[Settings];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QstTariffs_Canton_Code_PermitType_ChurchMember' AND object_id = OBJECT_ID(N'[dbo].[QstTariffs]'))
    DROP INDEX [IX_QstTariffs_Canton_Code_PermitType_ChurchMember] ON [dbo].[QstTariffs];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_QstTariffs_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo' AND object_id = OBJECT_ID(N'[dbo].[QstTariffs]'))
    DROP INDEX [IX_QstTariffs_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo] ON [dbo].[QstTariffs];
");

            migrationBuilder.DropColumn(
                name: "BvgCoordinationDeductionAnnual",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "BvgEmployeeRate",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "BvgEmployerRate",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "BvgPlanName",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "HolidayRate",
                table: "Lohns");

            migrationBuilder.RenameColumn(
                name: "HolidayEligible",
                table: "Lohns",
                newName: "Include13thSalary");

            migrationBuilder.RenameColumn(
                name: "EmployerAlv",
                table: "Lohns",
                newName: "ThirteenthSalaryAmount");

            migrationBuilder.RenameColumn(
                name: "EmployeeAlv",
                table: "Lohns",
                newName: "ShortTimeWorkDeduction");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "QstTariffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FinalizedAt",
                table: "Lohns",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "ApplyKTG",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CanteenDailyRate",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CanteenDays",
                table: "Lohns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CanteenDeduction",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EffectiveExpenses",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeAlv1",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeAlv2",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployeeKtg",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerAlv1",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerAlv2",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EmployerKtg",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Lohns",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ManualAdjustment",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PauschalExpenses",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrivateBenefitAmount",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyKTG",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Employees",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CompanyId",
                table: "Settings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CompanyId_Name",
                table: "Settings",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_CompanyId_Canton_Code_PermitType_ChurchMember",
                table: "QstTariffs",
                columns: new[] { "CompanyId", "Canton", "Code", "PermitType", "ChurchMember" });

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_CompanyId_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo",
                table: "QstTariffs",
                columns: new[] { "CompanyId", "Canton", "Code", "PermitType", "ChurchMember", "IncomeFrom", "IncomeTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settings_CompanyId",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_CompanyId_Name",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_QstTariffs_CompanyId_Canton_Code_PermitType_ChurchMember",
                table: "QstTariffs");

            migrationBuilder.DropIndex(
                name: "IX_QstTariffs_CompanyId_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo",
                table: "QstTariffs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "QstTariffs");

            migrationBuilder.DropColumn(
                name: "ApplyKTG",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "CanteenDailyRate",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "CanteenDays",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "CanteenDeduction",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EffectiveExpenses",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeAlv1",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeAlv2",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployeeKtg",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerAlv1",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerAlv2",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "EmployerKtg",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ManualAdjustment",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "PauschalExpenses",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "PrivateBenefitAmount",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyKTG",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "ThirteenthSalaryAmount",
                table: "Lohns",
                newName: "EmployerAlv");

            migrationBuilder.RenameColumn(
                name: "ShortTimeWorkDeduction",
                table: "Lohns",
                newName: "EmployeeAlv");

            migrationBuilder.RenameColumn(
                name: "Include13thSalary",
                table: "Lohns",
                newName: "HolidayEligible");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FinalizedAt",
                table: "Lohns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BvgCoordinationDeductionAnnual",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BvgEmployeeRate",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BvgEmployerRate",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BvgPlanName",
                table: "Lohns",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayRate",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Name",
                table: "Settings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_ChurchMember",
                table: "QstTariffs",
                columns: new[] { "Canton", "Code", "PermitType", "ChurchMember" });

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo",
                table: "QstTariffs",
                columns: new[] { "Canton", "Code", "PermitType", "ChurchMember", "IncomeFrom", "IncomeTo" },
                unique: true);
        }
    }
}
