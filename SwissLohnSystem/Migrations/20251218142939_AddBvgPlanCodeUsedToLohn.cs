using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBvgPlanCodeUsedToLohn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BvgPlanCodeUsed",
                table: "Lohns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BvgPlanCodeUsed",
                table: "Lohns");
        }
    }
}
