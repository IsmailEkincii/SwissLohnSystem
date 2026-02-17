using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyDefaultBvgPlanCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultBvgPlanCode",
                table: "Companies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultBvgPlanCode",
                table: "Companies");
        }
    }
}
