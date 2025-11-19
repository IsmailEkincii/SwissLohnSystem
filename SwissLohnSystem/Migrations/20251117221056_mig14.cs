using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class mig14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyAHV",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyALV",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBU",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyBVG",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyFAK",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyNBU",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApplyQST",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Canton",
                table: "Lohns",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChurchMember",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Lohns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HolidayEligible",
                table: "Lohns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayRate",
                table: "Lohns",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitType",
                table: "Lohns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WithholdingTaxCode",
                table: "Lohns",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyAHV",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyALV",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyBU",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyBVG",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyFAK",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyNBU",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ApplyQST",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "Canton",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "ChurchMember",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "HolidayEligible",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "HolidayRate",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "PermitType",
                table: "Lohns");

            migrationBuilder.DropColumn(
                name: "WithholdingTaxCode",
                table: "Lohns");
        }
    }
}
