using HR_system.Domain.SalaryCalculation;
using HR_system.DTOs.Salary;
using HR_system.DTOs.PayRoll;
using HR_system.Repositories;
using HR_system.Services.Interfaces;

namespace HR_system.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IMonthlyAttendanceRepository _monthlyAttendanceRepository;
        private readonly SalaryCalculator _salaryCalculator;

        private static readonly string[] ArabicMonthNames =
        {
            "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        public SalaryService(
            IPayrollRepository payrollRepository,
            IMonthlyAttendanceRepository monthlyAttendanceRepository,
            SalaryCalculator salaryCalculator)
        {
            _payrollRepository = payrollRepository;
            _monthlyAttendanceRepository = monthlyAttendanceRepository;
            _salaryCalculator = salaryCalculator;
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

            // Auto-populate MonthlyAttendance from daily records (won't overwrite manual entries)
            await _monthlyAttendanceRepository.PopulateFromDailyRecordsAsync(month, year, employeeIds);

            // Get all employees with their data
            var employees = await _payrollRepository.GetEmployeesWithRelatedDataAsync(employeeIds);

            // Get all related data for the month
            var attendances = await _payrollRepository.GetAttendanceRecordsAsync(month, year);
            var bonuses = await _payrollRepository.GetBonusRecordsAsync(month, year);
            var deductions = await _payrollRepository.GetDeductionRecordsAsync(month, year);
            var advances = await _payrollRepository.GetAdvanceRecordsAsync(month, year);
            var adjustments = await _payrollRepository.GetAttendanceAdjustmentRecordsAsync(month, year);
            var monthlyAttendances = await _payrollRepository.GetMonthlyAttendanceRecordsAsync(month, year);

            // Fetch previous month carry overs
            int prevMonth = month == 1 ? 12 : month - 1;
            int prevYear = month == 1 ? year - 1 : year;
            var prevMonthPayroll = await _payrollRepository.GetSavedPayrollAsync(prevMonth, prevYear);
            var prevCarryOvers = prevMonthPayroll?.Employees.ToDictionary(e => e.EmployeeId, e => e.SalaryCarryOver) ?? new Dictionary<int, decimal>();

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
                // Find monthly attendance record for this employee (if exists)
                var monthlyAttendance = monthlyAttendances
                    .FirstOrDefault(m => m.Employee_id == employee.Id);

                decimal previousMonthCarryOver = prevCarryOvers.GetValueOrDefault(employee.Id, 0m);

                var employeeSalary = _salaryCalculator.CalculateEmployeeSalary(
                    employee, attendances, bonuses, deductions, advances, adjustments,
                    workingDaysInMonth, holidaysInMonth, year, month, monthlyAttendance, previousMonthCarryOver);

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

        public async Task<bool> UpdatePayrollNoteAsync(UpdatePayrollNoteDto request)
        {
            return await _payrollRepository.UpdatePayrollNoteAsync(request);
        }

        public async Task<bool> DeleteMonthPayRollAsync(int month, int year)
        {
            return await _payrollRepository.DeleteMonthPayrollAsync(month, year);
        }

        public async Task<RecalculateSingleEmployeeResponseDto?> RecalculateSingleEmployeeAsync(int payrollId)
        {
            return await _payrollRepository.RecalculateSingleEmployeeAsync(payrollId, _salaryCalculator);
        }

        #endregion
    }
}
