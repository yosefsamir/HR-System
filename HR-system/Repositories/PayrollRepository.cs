using HR_system.Data;
using HR_system.Domain.SalaryCalculation;
using HR_system.DTOs.PayRoll;
using HR_system.DTOs.Salary;
using HR_system.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Repositories
{
    /// <summary>
    /// Repository for payroll data access operations
    /// </summary>
    public class PayrollRepository
    {
        private readonly ApplicationDbContext _context;

        public PayrollRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all employees with their related data
        /// </summary>
        public async Task<List<Employee>> GetEmployeesWithRelatedDataAsync(List<int> employeeIds)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Get attendance records for a specific month and year
        /// </summary>
        public async Task<List<Attendence>> GetAttendanceRecordsAsync(int month, int year)
        {
            return await _context.Attendences
                .Include(a => a.LateTime)
                .Include(a => a.EarlyDeparture)
                .Include(a => a.OverTime)
                .Where(a => a.Work_date.Month == month && a.Work_date.Year == year)
                .ToListAsync();
        }

        /// <summary>
        /// Get bonus records for a specific month and year
        /// </summary>
        public async Task<List<Bounes>> GetBonusRecordsAsync(int month, int year)
        {
            return await _context.Bounes
                .Where(b => b.Date.Month == month && b.Date.Year == year)
                .ToListAsync();
        }

        /// <summary>
        /// Get deduction records for a specific month and year
        /// </summary>
        public async Task<List<Deduction>> GetDeductionRecordsAsync(int month, int year)
        {
            return await _context.Deductions
                .Where(d => d.Date.Month == month && d.Date.Year == year)
                .ToListAsync();
        }

        /// <summary>
        /// Get advance records for a specific month and year
        /// </summary>
        public async Task<List<Advance>> GetAdvanceRecordsAsync(int month, int year)
        {
            return await _context.Advances
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .ToListAsync();
        }

        /// <summary>
        /// Get list of employee IDs who have any records in the specified month
        /// </summary>
        public async Task<List<int>> GetEmployeesWithRecordsInMonthAsync(int month, int year)
        {
            // Get employees from attendance
            var attendanceEmployees = await _context.Attendences
                .Where(a => a.Work_date.Month == month && a.Work_date.Year == year)
                .Select(a => a.Employee_id)
                .Distinct()
                .ToListAsync();

            // Get employees from bonuses
            var bonusEmployees = await _context.Bounes
                .Where(b => b.Date.Month == month && b.Date.Year == year)
                .Select(b => b.Employee_id)
                .Distinct()
                .ToListAsync();

            // Get employees from deductions
            var deductionEmployees = await _context.Deductions
                .Where(d => d.Date.Month == month && d.Date.Year == year)
                .Select(d => d.Employee_id)
                .Distinct()
                .ToListAsync();

            // Get employees from advances
            var advanceEmployees = await _context.Advances
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .Select(a => a.Employee_id)
                .Distinct()
                .ToListAsync();

            // Combine all unique employee IDs
            return attendanceEmployees
                .Union(bonusEmployees)
                .Union(deductionEmployees)
                .Union(advanceEmployees)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Check if payroll exists for month/year
        /// </summary>
        public async Task<PayRollExistsDto> CheckPayrollExistsAsync(int month, int year)
        {
            var records = await _context.PayRolls
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();

            return new PayRollExistsDto
            {
                Exists = records.Any(),
                RecordCount = records.Count,
                DateSaved = records.FirstOrDefault()?.DateSaved
            };
        }

        /// <summary>
        /// Save payroll records to database
        /// </summary>
        public async Task<SavePayRollResponseDto> SavePayrollAsync(SavePayRollRequestDto request, List<EmployeeSalaryResultDto> employeeSalaries)
        {
            var response = new SavePayRollResponseDto();

            try
            {
                int savedCount = 0;
                int updatedCount = 0;

                foreach (var empSalary in employeeSalaries)
                {
                    // Get paid salary from request (or default to net salary rounded up to nearest 5)
                    var empRequest = request.Employees.FirstOrDefault(e => e.EmployeeId == empSalary.EmployeeId);
                    decimal paidSalary = empRequest?.PaidSalary ?? RoundUpToNearest5(empSalary.NetSalary);
                    bool isPaid = empRequest?.IsPaid ?? false;

                    // Check if record exists
                    var existingPayRoll = await _context.PayRolls
                        .FirstOrDefaultAsync(p => p.Employee_id == empSalary.EmployeeId
                            && p.Month == request.Month
                            && p.Year == request.Year);

                    if (existingPayRoll != null)
                    {
                        // Update existing record
                        UpdatePayrollRecord(existingPayRoll, empSalary, request, paidSalary, isPaid);
                        updatedCount++;
                    }
                    else
                    {
                        // Create new record
                        var newPayRoll = CreatePayrollRecord(empSalary, request, paidSalary, isPaid);
                        _context.PayRolls.Add(newPayRoll);
                        savedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = $"تم حفظ {savedCount} سجل جديد وتحديث {updatedCount} سجل";
                response.SavedCount = savedCount;
                response.UpdatedCount = updatedCount;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"خطأ في الحفظ: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Get saved payroll data for month/year with optional shift and employee filters
        /// </summary>
        public async Task<SavedMonthlyPayRollDto?> GetSavedPayrollAsync(int month, int year, int? shiftId = null, int? employeeId = null)
        {
            var query = _context.PayRolls
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .Where(p => p.Month == month && p.Year == year);

            // Apply shift filter
            if (shiftId.HasValue && shiftId.Value > 0)
            {
                query = query.Where(p => p.Employee != null && p.Employee.Shift_id == shiftId.Value);
            }

            // Apply employee filter
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(p => p.Employee_id == employeeId.Value);
            }

            var records = await query.ToListAsync();

            if (!records.Any()) return null;

            var result = new SavedMonthlyPayRollDto
            {
                Month = month,
                Year = year,
                MonthName = GetArabicMonthName(month),
                WorkingDays = records.First().WorkingDaysInMonth,
                Holidays = records.First().HolidaysInMonth,
                DateSaved = records.First().DateSaved,
                TotalEmployees = records.Count,
                TotalNetSalaries = records.Sum(r => r.NetSalary),
                TotalPaidSalaries = records.Sum(r => r.ActualPaidAmount),
                TotalBonuses = records.Sum(r => r.TotalBonuses),
                TotalDeductions = records.Sum(r => r.TotalDeductions),
                TotalAdvances = records.Sum(r => r.TotalAdvances),
                TotalOvertimeAmount = records.Sum(r => r.OvertimeAmount),
                TotalLateTimeDeduction = records.Sum(r => r.LateTimeDeduction),
                TotalEarlyDepartureDeduction = records.Sum(r => r.EarlyDepartureDeduction),
                PaidCount = records.Count(r => r.IsPaid),
                UnpaidCount = records.Count(r => !r.IsPaid),
                Employees = records.Select(r => new SavedPayRollDto
                {
                    Id = r.Id,
                    EmployeeId = r.Employee_id,
                    EmployeeName = r.EmployeeName,
                    EmployeeCode = r.EmployeeCode,
                    DepartmentName = r.DepartmentName ?? "",
                    ShiftName = r.ShiftName ?? "",
                    PresentDays = r.ActualPresentDays,
                    AbsentDays = r.AbsentDays,
                    ActualWorkedMinutes = r.ActualWorkedMinutes,
                    ActualWorkedHours = r.ActualWorkedHours,
                    OvertimeMinutes = r.OvertimeMinutes,
                    OvertimeHours = r.OvertimeHours,
                    OvertimeMultiplier = r.OvertimeMultiplier,
                    LateTimeMinutes = r.LateTimeMinutes,
                    LateTimeHours = r.LateTimeHours,
                    LateTimeMultiplier = r.LateTimeMultiplier,
                    EarlyDepartureMinutes = r.EarlyDepartureMinutes,
                    EarlyDepartureHours = r.EarlyDepartureHours,
                    EarlyDepartureMultiplier = r.EarlyDepartureMultiplier,
                    BaseSalary = r.BaseSalary,
                    SalaryPerHour = r.SalaryPerHour,
                    SalaryPerDay = r.SalaryPerDay,
                    SalaryCalculationType = r.SalaryCalculationType,
                    SalaryCalculationTypeDisplay = r.SalaryCalculationTypeDisplay,
                    WorkedHoursSalary = r.WorkedHoursSalary,
                    OvertimeAmount = r.OvertimeAmount,
                    LateTimeDeduction = r.LateTimeDeduction,
                    EarlyDepartureDeduction = r.EarlyDepartureDeduction,
                    Bonuses = r.TotalBonuses,
                    Deductions = r.TotalDeductions,
                    Advances = r.TotalAdvances,
                    GrossSalary = r.GrossSalary,
                    TotalDeductionsAmount = r.TotalDeductionsAmount,
                    NetSalary = r.NetSalary,
                    PaidSalary = r.ActualPaidAmount,
                    IsPaid = r.IsPaid,
                    DateSaved = r.DateSaved
                }).ToList()
            };

            return result;
        }

        /// <summary>
        /// Update paid salary for single employee
        /// </summary>
        public async Task<bool> UpdatePaidSalaryAsync(UpdatePaidSalaryDto request)
        {
            var payRoll = await _context.PayRolls.FindAsync(request.PayRollId);
            if (payRoll == null) return false;

            // Round up paid salary to nearest 5
            payRoll.ActualPaidAmount = RoundUpToNearest5(request.PaidSalary);
            payRoll.IsPaid = request.IsPaid;
            payRoll.DateSaved = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Delete all payroll records for month/year
        /// </summary>
        public async Task<bool> DeleteMonthPayrollAsync(int month, int year)
        {
            var records = await _context.PayRolls
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();

            if (!records.Any()) return false;

            _context.PayRolls.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }

        private PayRoll CreatePayrollRecord(EmployeeSalaryResultDto empSalary, SavePayRollRequestDto request, decimal paidSalary, bool isPaid)
        {
            return new PayRoll
            {
                Employee_id = empSalary.EmployeeId,
                Month = request.Month,
                Year = request.Year,

                // Employee Info
                EmployeeName = empSalary.EmployeeName,
                EmployeeCode = empSalary.EmployeeCode,
                DepartmentName = empSalary.DepartmentName,
                ShiftName = empSalary.ShiftName,

                // Month Configuration
                WorkingDaysInMonth = request.WorkingDaysInMonth,
                HolidaysInMonth = request.HolidaysInMonth,

                // Base Salary Info
                BaseSalary = empSalary.BaseSalary,
                SalaryPerHour = empSalary.SalaryPerHour,
                SalaryPerDay = empSalary.SalaryPerDay,
                SalaryCalculationType = empSalary.SalaryCalculationType,
                SalaryCalculationTypeDisplay = empSalary.SalaryCalculationTypeDisplay,

                // Working Hours Info
                ShiftHoursPerDay = empSalary.ShiftHoursPerDay,
                ExpectedWorkingHours = empSalary.ExpectedWorkingHours,

                // Attendance Summary
                ActualPresentDays = empSalary.ActualPresentDays,
                AbsentDays = empSalary.AbsentDays,
                ActualWorkedMinutes = empSalary.ActualWorkedMinutes,
                ActualWorkedHours = empSalary.ActualWorkedHours,

                // Overtime Details
                OvertimeMinutes = empSalary.OvertimeMinutes,
                OvertimeHours = empSalary.OvertimeHours,
                OvertimeMultiplier = empSalary.OvertimeMultiplier,
                OvertimeAmount = empSalary.OvertimeAmount,

                // Late Time Details
                LateTimeMinutes = empSalary.LateTimeMinutes,
                LateTimeHours = empSalary.LateTimeHours,
                LateTimeMultiplier = empSalary.LateTimeMultiplier,
                LateTimeDeduction = empSalary.LateTimeDeduction,

                // Early Departure Details
                EarlyDepartureMinutes = empSalary.EarlyDepartureMinutes,
                EarlyDepartureHours = empSalary.EarlyDepartureHours,
                EarlyDepartureMultiplier = empSalary.EarlyDepartureMultiplier,
                EarlyDepartureDeduction = empSalary.EarlyDepartureDeduction,

                // Net Time Difference
                NetTimeDifferenceHours = empSalary.NetTimeDifferenceHours,
                NetTimeDifferenceAmount = empSalary.NetTimeDifferenceAmount,

                // Permission
                PermissionMinutes = empSalary.PermissionMinutes,
                PermissionHours = empSalary.PermissionHours,

                // Financial Summary
                TotalBonuses = empSalary.TotalBonuses,
                TotalDeductions = empSalary.TotalDeductions,
                TotalAdvances = empSalary.TotalAdvances,

                // Worked Hours Salary
                WorkedHoursSalary = empSalary.WorkedHoursSalary,

                // Final Calculations
                GrossSalary = empSalary.GrossSalary,
                TotalDeductionsAmount = empSalary.TotalDeductionsAmount,
                NetSalary = empSalary.NetSalary,

                // Payment Status
                ActualPaidAmount = paidSalary,
                IsPaid = isPaid,
                DateSaved = DateTime.Now
            };
        }

        private void UpdatePayrollRecord(PayRoll payRoll, EmployeeSalaryResultDto empSalary, SavePayRollRequestDto request, decimal paidSalary, bool isPaid)
        {
            // Employee Info
            payRoll.EmployeeName = empSalary.EmployeeName;
            payRoll.EmployeeCode = empSalary.EmployeeCode;
            payRoll.DepartmentName = empSalary.DepartmentName;
            payRoll.ShiftName = empSalary.ShiftName;

            // Month Configuration
            payRoll.WorkingDaysInMonth = request.WorkingDaysInMonth;
            payRoll.HolidaysInMonth = request.HolidaysInMonth;

            // Base Salary Info
            payRoll.BaseSalary = empSalary.BaseSalary;
            payRoll.SalaryPerHour = empSalary.SalaryPerHour;
            payRoll.SalaryPerDay = empSalary.SalaryPerDay;
            payRoll.SalaryCalculationType = empSalary.SalaryCalculationType;
            payRoll.SalaryCalculationTypeDisplay = empSalary.SalaryCalculationTypeDisplay;

            // Working Hours Info
            payRoll.ShiftHoursPerDay = empSalary.ShiftHoursPerDay;
            payRoll.ExpectedWorkingHours = empSalary.ExpectedWorkingHours;

            // Attendance Summary
            payRoll.ActualPresentDays = empSalary.ActualPresentDays;
            payRoll.AbsentDays = empSalary.AbsentDays;
            payRoll.ActualWorkedMinutes = empSalary.ActualWorkedMinutes;
            payRoll.ActualWorkedHours = empSalary.ActualWorkedHours;

            // Overtime Details
            payRoll.OvertimeMinutes = empSalary.OvertimeMinutes;
            payRoll.OvertimeHours = empSalary.OvertimeHours;
            payRoll.OvertimeMultiplier = empSalary.OvertimeMultiplier;
            payRoll.OvertimeAmount = empSalary.OvertimeAmount;

            // Late Time Details
            payRoll.LateTimeMinutes = empSalary.LateTimeMinutes;
            payRoll.LateTimeHours = empSalary.LateTimeHours;
            payRoll.LateTimeMultiplier = empSalary.LateTimeMultiplier;
            payRoll.LateTimeDeduction = empSalary.LateTimeDeduction;

            // Early Departure Details
            payRoll.EarlyDepartureMinutes = empSalary.EarlyDepartureMinutes;
            payRoll.EarlyDepartureHours = empSalary.EarlyDepartureHours;
            payRoll.EarlyDepartureMultiplier = empSalary.EarlyDepartureMultiplier;
            payRoll.EarlyDepartureDeduction = empSalary.EarlyDepartureDeduction;

            // Net Time Difference
            payRoll.NetTimeDifferenceHours = empSalary.NetTimeDifferenceHours;
            payRoll.NetTimeDifferenceAmount = empSalary.NetTimeDifferenceAmount;

            // Permission
            payRoll.PermissionMinutes = empSalary.PermissionMinutes;
            payRoll.PermissionHours = empSalary.PermissionHours;

            // Financial Summary
            payRoll.TotalBonuses = empSalary.TotalBonuses;
            payRoll.TotalDeductions = empSalary.TotalDeductions;
            payRoll.TotalAdvances = empSalary.TotalAdvances;

            // Worked Hours Salary
            payRoll.WorkedHoursSalary = empSalary.WorkedHoursSalary;

            // Final Calculations
            payRoll.GrossSalary = empSalary.GrossSalary;
            payRoll.TotalDeductionsAmount = empSalary.TotalDeductionsAmount;
            payRoll.NetSalary = empSalary.NetSalary;

            // Payment Status
            payRoll.ActualPaidAmount = paidSalary;
            payRoll.IsPaid = isPaid;
            payRoll.DateSaved = DateTime.Now;
        }

        private string GetArabicMonthName(int month)
        {
            string[] monthNames = { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
            return monthNames[month];
        }

        /// <summary>
        /// Round up salary to nearest 5 (e.g., 103 → 105, 107 → 110)
        /// </summary>
        private decimal RoundUpToNearest5(decimal amount)
        {
            return Math.Ceiling(amount / 5) * 5;
        }

        /// <summary>
        /// Recalculate salary for a single employee and update the payroll record
        /// </summary>
        public async Task<RecalculateSingleEmployeeResponseDto?> RecalculateSingleEmployeeAsync(int payrollId, SalaryCalculator salaryCalculator)
        {
            // Get the existing payroll record
            var payroll = await _context.PayRolls
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Shift)
                .FirstOrDefaultAsync(p => p.Id == payrollId);

            if (payroll == null || payroll.Employee == null) return null;

            var employee = payroll.Employee;
            var month = payroll.Month;
            var year = payroll.Year;
            var workingDaysInMonth = payroll.WorkingDaysInMonth;
            var holidaysInMonth = payroll.HolidaysInMonth;

            // Get related data for this employee in this month
            var attendances = await _context.Attendences
                .Include(a => a.LateTime)
                .Include(a => a.EarlyDeparture)
                .Include(a => a.OverTime)
                .Where(a => a.Work_date.Month == month && a.Work_date.Year == year && a.Employee_id == employee.Id)
                .ToListAsync();

            var bonuses = await _context.Bounes
                .Where(b => b.Date.Month == month && b.Date.Year == year && b.Employee_id == employee.Id)
                .ToListAsync();

            var deductions = await _context.Deductions
                .Where(d => d.Date.Month == month && d.Date.Year == year && d.Employee_id == employee.Id)
                .ToListAsync();

            var advances = await _context.Advances
                .Where(a => a.Date.Month == month && a.Date.Year == year && a.Employee_id == employee.Id)
                .ToListAsync();

            // Recalculate salary
            var empSalary = salaryCalculator.CalculateEmployeeSalary(
                employee, attendances, bonuses, deductions, advances,
                workingDaysInMonth, holidaysInMonth, year, month);

            // Update payroll record
            payroll.EmployeeName = empSalary.EmployeeName;
            payroll.EmployeeCode = empSalary.EmployeeCode;
            payroll.DepartmentName = empSalary.DepartmentName;
            payroll.ShiftName = empSalary.ShiftName;
            payroll.BaseSalary = empSalary.BaseSalary;
            payroll.SalaryPerHour = empSalary.SalaryPerHour;
            payroll.SalaryPerDay = empSalary.SalaryPerDay;
            payroll.SalaryCalculationType = empSalary.SalaryCalculationType;
            payroll.SalaryCalculationTypeDisplay = empSalary.SalaryCalculationTypeDisplay;
            payroll.ShiftHoursPerDay = empSalary.ShiftHoursPerDay;
            payroll.ExpectedWorkingHours = empSalary.ExpectedWorkingHours;
            payroll.ActualPresentDays = empSalary.ActualPresentDays;
            payroll.AbsentDays = empSalary.AbsentDays;
            payroll.ActualWorkedMinutes = empSalary.ActualWorkedMinutes;
            payroll.ActualWorkedHours = empSalary.ActualWorkedHours;
            payroll.OvertimeMinutes = empSalary.OvertimeMinutes;
            payroll.OvertimeHours = empSalary.OvertimeHours;
            payroll.OvertimeMultiplier = empSalary.OvertimeMultiplier;
            payroll.OvertimeAmount = empSalary.OvertimeAmount;
            payroll.LateTimeMinutes = empSalary.LateTimeMinutes;
            payroll.LateTimeHours = empSalary.LateTimeHours;
            payroll.LateTimeMultiplier = empSalary.LateTimeMultiplier;
            payroll.LateTimeDeduction = empSalary.LateTimeDeduction;
            payroll.EarlyDepartureMinutes = empSalary.EarlyDepartureMinutes;
            payroll.EarlyDepartureHours = empSalary.EarlyDepartureHours;
            payroll.EarlyDepartureMultiplier = empSalary.EarlyDepartureMultiplier;
            payroll.EarlyDepartureDeduction = empSalary.EarlyDepartureDeduction;
            payroll.NetTimeDifferenceHours = empSalary.NetTimeDifferenceHours;
            payroll.NetTimeDifferenceAmount = empSalary.NetTimeDifferenceAmount;
            payroll.PermissionMinutes = empSalary.PermissionMinutes;
            payroll.PermissionHours = empSalary.PermissionHours;
            payroll.TotalBonuses = empSalary.TotalBonuses;
            payroll.TotalDeductions = empSalary.TotalDeductions;
            payroll.TotalAdvances = empSalary.TotalAdvances;
            payroll.WorkedHoursSalary = empSalary.WorkedHoursSalary;
            payroll.GrossSalary = empSalary.GrossSalary;
            payroll.TotalDeductionsAmount = empSalary.TotalDeductionsAmount;
            payroll.NetSalary = empSalary.NetSalary;
            // Round up paid salary to nearest 5 (e.g., 103 → 105, 107 → 110)
            payroll.ActualPaidAmount = RoundUpToNearest5(empSalary.NetSalary);
            payroll.DateSaved = DateTime.Now;

            await _context.SaveChangesAsync();

            // Return updated data
            return new RecalculateSingleEmployeeResponseDto
            {
                Id = payroll.Id,
                EmployeeId = payroll.Employee_id,
                EmployeeName = payroll.EmployeeName,
                EmployeeCode = payroll.EmployeeCode,
                DepartmentName = payroll.DepartmentName ?? "",
                ShiftName = payroll.ShiftName ?? "",
                PresentDays = payroll.ActualPresentDays,
                AbsentDays = payroll.AbsentDays,
                ActualWorkedMinutes = payroll.ActualWorkedMinutes,
                ActualWorkedHours = payroll.ActualWorkedHours,
                OvertimeMinutes = payroll.OvertimeMinutes,
                OvertimeHours = payroll.OvertimeHours,
                OvertimeMultiplier = payroll.OvertimeMultiplier,
                OvertimeAmount = payroll.OvertimeAmount,
                LateTimeMinutes = payroll.LateTimeMinutes,
                LateTimeHours = payroll.LateTimeHours,
                LateTimeMultiplier = payroll.LateTimeMultiplier,
                LateTimeDeduction = payroll.LateTimeDeduction,
                EarlyDepartureMinutes = payroll.EarlyDepartureMinutes,
                EarlyDepartureHours = payroll.EarlyDepartureHours,
                EarlyDepartureMultiplier = payroll.EarlyDepartureMultiplier,
                EarlyDepartureDeduction = payroll.EarlyDepartureDeduction,
                BaseSalary = payroll.BaseSalary,
                SalaryPerHour = payroll.SalaryPerHour,
                SalaryPerDay = payroll.SalaryPerDay,
                SalaryCalculationType = payroll.SalaryCalculationType,
                SalaryCalculationTypeDisplay = payroll.SalaryCalculationTypeDisplay,
                WorkedHoursSalary = payroll.WorkedHoursSalary,
                Bonuses = payroll.TotalBonuses,
                Deductions = payroll.TotalDeductions,
                Advances = payroll.TotalAdvances,
                GrossSalary = payroll.GrossSalary,
                TotalDeductionsAmount = payroll.TotalDeductionsAmount,
                NetSalary = payroll.NetSalary,
                PaidSalary = payroll.ActualPaidAmount,
                IsPaid = payroll.IsPaid,
                DateSaved = payroll.DateSaved
            };
        }
    }
}