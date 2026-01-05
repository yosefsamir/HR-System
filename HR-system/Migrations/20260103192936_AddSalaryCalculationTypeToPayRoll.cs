using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryCalculationTypeToPayRoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalaryCalculationType",
                table: "PayRolls",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalaryCalculationTypeDisplay",
                table: "PayRolls",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryPerDay",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalaryCalculationType",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "SalaryCalculationTypeDisplay",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "SalaryPerDay",
                table: "PayRolls");
        }
    }
}
