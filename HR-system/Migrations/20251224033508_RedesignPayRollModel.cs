using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_system.Migrations
{
    /// <inheritdoc />
    public partial class RedesignPayRollModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Shifts_Shift_id",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Date_saved",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "Number_of_absents_day",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "Number_of_holidays",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "Number_of_worked_days",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "Total_added_hour",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "Total_hours_in_this_month",
                table: "PayRolls");

            migrationBuilder.RenameColumn(
                name: "Netsalary",
                table: "PayRolls",
                newName: "NetSalary");

            migrationBuilder.RenameColumn(
                name: "Total_worked_hours",
                table: "PayRolls",
                newName: "WorkingDaysInMonth");

            migrationBuilder.RenameColumn(
                name: "Total_overtime",
                table: "PayRolls",
                newName: "HolidaysInMonth");

            migrationBuilder.RenameColumn(
                name: "Total_lateTime",
                table: "PayRolls",
                newName: "ActualPresentDays");

            migrationBuilder.RenameColumn(
                name: "Total_hours_in_work_time",
                table: "PayRolls",
                newName: "AbsentDays");

            migrationBuilder.RenameColumn(
                name: "Total_deduction",
                table: "PayRolls",
                newName: "WorkedHoursSalary");

            migrationBuilder.RenameColumn(
                name: "Total_bounes",
                table: "PayRolls",
                newName: "TotalDeductionsAmount");

            migrationBuilder.RenameColumn(
                name: "Total_advance",
                table: "PayRolls",
                newName: "TotalDeductions");

            migrationBuilder.RenameColumn(
                name: "Salary_per_hour",
                table: "PayRolls",
                newName: "TotalBonuses");

            migrationBuilder.RenameColumn(
                name: "Salary_actual_paid",
                table: "PayRolls",
                newName: "TotalAdvances");

            migrationBuilder.RenameColumn(
                name: "Rate_overtime",
                table: "PayRolls",
                newName: "ShiftHoursPerDay");

            migrationBuilder.RenameColumn(
                name: "Rate_lateTime",
                table: "PayRolls",
                newName: "SalaryPerHour");

            migrationBuilder.RenameColumn(
                name: "Payed",
                table: "PayRolls",
                newName: "IsPaid");

            migrationBuilder.RenameColumn(
                name: "Base_salary",
                table: "PayRolls",
                newName: "PermissionMinutes");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualPaidAmount",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWorkedHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWorkedMinutes",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSaved",
                table: "PayRolls",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "PayRolls",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "PayRolls",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "PayRolls",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedWorkingHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateTimeDeduction",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateTimeHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateTimeMinutes",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LateTimeMultiplier",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTimeDifferenceAmount",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetTimeDifferenceHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeAmount",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeMinutes",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeMultiplier",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PermissionHours",
                table: "PayRolls",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShiftName",
                table: "PayRolls",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Shift_id",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Shifts_Shift_id",
                table: "Employees",
                column: "Shift_id",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Shifts_Shift_id",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ActualPaidAmount",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "ActualWorkedHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "ActualWorkedMinutes",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "DateSaved",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "ExpectedWorkingHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "LateTimeDeduction",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "LateTimeHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "LateTimeMinutes",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "LateTimeMultiplier",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "NetTimeDifferenceAmount",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "NetTimeDifferenceHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "OvertimeAmount",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "OvertimeMinutes",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "OvertimeMultiplier",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "PermissionHours",
                table: "PayRolls");

            migrationBuilder.DropColumn(
                name: "ShiftName",
                table: "PayRolls");

            migrationBuilder.RenameColumn(
                name: "NetSalary",
                table: "PayRolls",
                newName: "Netsalary");

            migrationBuilder.RenameColumn(
                name: "WorkingDaysInMonth",
                table: "PayRolls",
                newName: "Total_worked_hours");

            migrationBuilder.RenameColumn(
                name: "WorkedHoursSalary",
                table: "PayRolls",
                newName: "Total_deduction");

            migrationBuilder.RenameColumn(
                name: "TotalDeductionsAmount",
                table: "PayRolls",
                newName: "Total_bounes");

            migrationBuilder.RenameColumn(
                name: "TotalDeductions",
                table: "PayRolls",
                newName: "Total_advance");

            migrationBuilder.RenameColumn(
                name: "TotalBonuses",
                table: "PayRolls",
                newName: "Salary_per_hour");

            migrationBuilder.RenameColumn(
                name: "TotalAdvances",
                table: "PayRolls",
                newName: "Salary_actual_paid");

            migrationBuilder.RenameColumn(
                name: "ShiftHoursPerDay",
                table: "PayRolls",
                newName: "Rate_overtime");

            migrationBuilder.RenameColumn(
                name: "SalaryPerHour",
                table: "PayRolls",
                newName: "Rate_lateTime");

            migrationBuilder.RenameColumn(
                name: "PermissionMinutes",
                table: "PayRolls",
                newName: "Base_salary");

            migrationBuilder.RenameColumn(
                name: "IsPaid",
                table: "PayRolls",
                newName: "Payed");

            migrationBuilder.RenameColumn(
                name: "HolidaysInMonth",
                table: "PayRolls",
                newName: "Total_overtime");

            migrationBuilder.RenameColumn(
                name: "ActualPresentDays",
                table: "PayRolls",
                newName: "Total_lateTime");

            migrationBuilder.RenameColumn(
                name: "AbsentDays",
                table: "PayRolls",
                newName: "Total_hours_in_work_time");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_saved",
                table: "PayRolls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Number_of_absents_day",
                table: "PayRolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Number_of_holidays",
                table: "PayRolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Number_of_worked_days",
                table: "PayRolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Total_added_hour",
                table: "PayRolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Total_hours_in_this_month",
                table: "PayRolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Shift_id",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Shifts_Shift_id",
                table: "Employees",
                column: "Shift_id",
                principalTable: "Shifts",
                principalColumn: "Id");
        }
    }
}
