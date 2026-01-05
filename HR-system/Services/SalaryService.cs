using HR_system.Data;
using HR_system.Domain.SalaryCalculation;
using HR_system.DTOs.Salary;
using HR_system.DTOs.PayRoll;
using HR_system.Repositories;
using HR_system.Services.Interfaces;

namespace HR_system.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly PayrollRepository _payrollRepository;
        private readonly SalaryCalculator _salaryCalculator;

        private static readonly string[] ArabicMonthNames =
        {
            "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        public SalaryService(ApplicationDbContext context)
        {
            _payrollRepository = new PayrollRepository(context);
            _salaryCalculator = new SalaryCalculator();
        }

        #region Main Calculation Methods

        public async Task<AllEmployeesSalaryResultDto> CalculateAllEmployeesSalariesAsync(SalaryCalculationRequestDto request)
        {
            return await CalculateAllEmployeesSalariesAsync(
                request.Month,
                request.Year,
                request.WorkingDaysInMonth,
                request.HolidaysInMonth);
        }

        public async Task<AllEmployeesSalaryResultDto> CalculateAllEmployeesSalariesAsync(
            int month, int year, int workingDaysInMonth, int holidaysInMonth = 0)
        {
            // Get employee IDs with records in this month
            var employeeIds = await _payrollRepository.GetEmployeesWithRecordsInMonthAsync(month, year);

            // Get all employees with their data
            var employees = await _payrollRepository.GetEmployeesWithRelatedDataAsync(employeeIds);

            // Get all related data for the month
            var attendances = await _payrollRepository.GetAttendanceRecordsAsync(month, year);
            var bonuses = await _payrollRepository.GetBonusRecordsAsync(month, year);
            var deductions = await _payrollRepository.GetDeductionRecordsAsync(month, year);
            var advances = await _payrollRepository.GetAdvanceRecordsAsync(month, year);

            // Initialize result
            var result = new AllEmployeesSalaryResultDto
            {
                Month = month,
                Year = year,
                MonthName = ArabicMonthNames[month],
                WorkingDaysInMonth = workingDaysInMonth,
                HolidaysInMonth = holidaysInMonth,
                Employees = new List<EmployeeSalaryResultDto>()
            };

            // Calculate salary for each employee
            foreach (var employee in employees)
            {
                var employeeSalary = _salaryCalculator.CalculateEmployeeSalary(
                    employee, attendances, bonuses, deductions, advances,
                    workingDaysInMonth, holidaysInMonth, year, month);

                result.Employees.Add(employeeSalary);
            }

            // Calculate totals
            _salaryCalculator.CalculateTotals(result);

            return result;
        }

        #endregion

        #region PayRoll Save/Get Methods

        public async Task<PayRollExistsDto> PayRollExistsAsync(int month, int year)
        {
            return await _payrollRepository.CheckPayrollExistsAsync(month, year);
        }

        public async Task<SavePayRollResponseDto> SavePayRollAsync(SavePayRollRequestDto request)
        {
            // First calculate all salaries
            var salaryResult = await CalculateAllEmployeesSalariesAsync(
                request.Month,
                request.Year,
                request.WorkingDaysInMonth,
                request.HolidaysInMonth);

            // Save using repository
            return await _payrollRepository.SavePayrollAsync(request, salaryResult.Employees);
        }

        public async Task<SavedMonthlyPayRollDto?> GetSavedPayRollAsync(int month, int year, int? shiftId = null, int? employeeId = null)
        {
            return await _payrollRepository.GetSavedPayrollAsync(month, year, shiftId, employeeId);
        }

        public async Task<bool> UpdatePaidSalaryAsync(UpdatePaidSalaryDto request)
        {
            return await _payrollRepository.UpdatePaidSalaryAsync(request);
        }

        public async Task<bool> DeleteMonthPayRollAsync(int month, int year)
        {
            return await _payrollRepository.DeleteMonthPayrollAsync(month, year);
        }

        #endregion
    }
}
