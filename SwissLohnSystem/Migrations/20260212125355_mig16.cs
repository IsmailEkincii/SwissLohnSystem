using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class mig16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Settings",
                type: "nvarchar(200)",
                maxLength: 200,
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Settings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "BvgPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlanBaseCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CoordinationDedAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EntryThresholdAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpperLimitAnnual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate25_34_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate25_34_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate35_44_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate35_44_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate45_54_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate45_54_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate55_65_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate55_65_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BvgPlans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BvgPlans_CompanyId_PlanCode",
                table: "BvgPlans",
                columns: new[] { "CompanyId", "PlanCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BvgPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Settings");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Settings",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
