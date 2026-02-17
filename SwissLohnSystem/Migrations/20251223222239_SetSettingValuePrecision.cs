using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwissLohnSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class SetSettingValuePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_IncomeFrom_IncomeTo",
                table: "QstTariffs");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Settings",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo",
                table: "QstTariffs",
                columns: new[] { "Canton", "Code", "PermitType", "ChurchMember", "IncomeFrom", "IncomeTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_ChurchMember_IncomeFrom_IncomeTo",
                table: "QstTariffs");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Settings",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.CreateIndex(
                name: "IX_QstTariffs_Canton_Code_PermitType_IncomeFrom_IncomeTo",
                table: "QstTariffs",
                columns: new[] { "Canton", "Code", "PermitType", "IncomeFrom", "IncomeTo" });
        }
    }
}
