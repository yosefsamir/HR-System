using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PresentDays = table.Column<int>(type: "int", nullable: false),
                    AbsentDays = table.Column<int>(type: "int", nullable: false),
                    WorkedMinutes = table.Column<int>(type: "int", nullable: false),
                    LateMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyDepartureMinutes = table.Column<int>(type: "int", nullable: false),
                    PermissionMinutes = table.Column<int>(type: "int", nullable: false),
                    IsManuallyEntered = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyAttendances_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAttendances_Employee_id_Month_Year",
                table: "MonthlyAttendances",
                columns: new[] { "Employee_id", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyAttendances");
        }
    }
}
