using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Shift_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    End_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Minutes_allow_attendence = table.Column<int>(type: "int", nullable: false),
                    Minutes_allow_departure = table.Column<int>(type: "int", nullable: false),
                    StandardHours = table.Column<decimal>(type: "decimal(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emp_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Department_id = table.Column<int>(type: "int", nullable: true),
                    Shift_id = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rate_overtime_multiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Rate_latetime_multiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_Department_id",
                        column: x => x.Department_id,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employees_Shifts_Shift_id",
                        column: x => x.Shift_id,
                        principalTable: "Shifts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Advances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advances_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Work_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Check_In_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    Check_out_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    Worked_minutes = table.Column<int>(type: "int", nullable: false),
                    Is_Absent = table.Column<bool>(type: "bit", nullable: false),
                    Permission_time = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendences_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bounes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bounes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bounes_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deductions_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayRolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee_id = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Number_of_holidays = table.Column<int>(type: "int", nullable: false),
                    Base_salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Number_of_worked_days = table.Column<int>(type: "int", nullable: false),
                    Number_of_absents_day = table.Column<int>(type: "int", nullable: false),
                    Total_hours_in_this_month = table.Column<int>(type: "int", nullable: false),
                    Salary_per_hour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_hours_in_work_time = table.Column<int>(type: "int", nullable: false),
                    Total_overtime = table.Column<int>(type: "int", nullable: false),
                    Rate_overtime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_lateTime = table.Column<int>(type: "int", nullable: false),
                    Rate_lateTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_added_hour = table.Column<int>(type: "int", nullable: false),
                    Total_worked_hours = table.Column<int>(type: "int", nullable: false),
                    Total_advance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_bounes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_deduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Netsalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Salary_actual_paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Payed = table.Column<bool>(type: "bit", nullable: false),
                    Date_saved = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayRolls_Employees_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LateTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendence_id = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LateTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LateTimes_Attendences_Attendence_id",
                        column: x => x.Attendence_id,
                        principalTable: "Attendences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendence_id = table.Column<int>(type: "int", nullable: false),
                    Minutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverTimes_Attendences_Attendence_id",
                        column: x => x.Attendence_id,
                        principalTable: "Attendences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advances_Employee_id",
                table: "Advances",
                column: "Employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendences_Employee_id",
                table: "Attendences",
                column: "Employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bounes_Employee_id",
                table: "Bounes",
                column: "Employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Deductions_Employee_id",
                table: "Deductions",
                column: "Employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Department_id",
                table: "Employees",
                column: "Department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Shift_id",
                table: "Employees",
                column: "Shift_id");

            migrationBuilder.CreateIndex(
                name: "IX_LateTimes_Attendence_id",
                table: "LateTimes",
                column: "Attendence_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OverTimes_Attendence_id",
                table: "OverTimes",
                column: "Attendence_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayRolls_Employee_id",
                table: "PayRolls",
                column: "Employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advances");

            migrationBuilder.DropTable(
                name: "Bounes");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DropTable(
                name: "LateTimes");

            migrationBuilder.DropTable(
                name: "OverTimes");

            migrationBuilder.DropTable(
                name: "PayRolls");

            migrationBuilder.DropTable(
                name: "Attendences");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Shifts");
        }
    }
}
