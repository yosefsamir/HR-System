using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryCarryOverToPayRoll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SalaryCarryOver",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalaryCarryOver",
                table: "PayRolls");
        }
    }
}
