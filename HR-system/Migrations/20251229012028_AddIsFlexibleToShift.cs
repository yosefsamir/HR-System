using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFlexibleToShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFlexible",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFlexible",
                table: "Shifts");
        }
    }
}
