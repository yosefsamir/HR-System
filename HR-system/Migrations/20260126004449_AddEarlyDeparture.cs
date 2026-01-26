using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class AddEarlyDeparture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EarlyDepartureMultiplier",
                table: "Shifts",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyDepartureDeduction",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyDepartureHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyDepartureMinutes",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlyDepartureMultiplier",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EarlyDepartures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendence_id = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarlyDepartures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarlyDepartures_Attendences_Attendence_id",
                        column: x => x.Attendence_id,
                        principalTable: "Attendences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EarlyDepartures_Attendence_id",
                table: "EarlyDepartures",
                column: "Attendence_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarlyDepartures");

            migrationBuilder.DropColumn(
                name: "EarlyDepartureMultiplier",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "EarlyDepartureDeduction",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EarlyDepartureHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EarlyDepartureMinutes",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EarlyDepartureMultiplier",
                table: "PayRolls");
        }
    }
}
