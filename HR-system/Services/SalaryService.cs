using HR_system.Data;
using HR_system.DTOs.Salary;
using HR_system.DTOs.PayRoll;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly ApplicationDbContext _context;
        
        private static readonly string[] ArabicMonthNames = 
        { 
            "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", 
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" 
        };

        public SalaryService(ApplicationDbContext context)
        {
            _context = context;
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
            // Get all employee IDs with records in this month
            var employeeIds = await GetEmployeesWithRecordsInMonthAsync(month, year);
            
            // Get all employees with their data
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();
            
            // Get date range for the month
            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            
            // Get all attendance records for the month
            var attendances = await _context.Attendences
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .Where(a => a.Work_date.Month == month && a.Work_date.Year == year)
                .ToListAsync();
            
            // Get all bonuses for the month
            var bonuses = await _context.Bounes
                .Where(b => b.Date.Month == month && b.Date.Year == year)
                .ToListAsync();
            
            // Get all deductions for the month
            var deductions = await _context.Deductions
                .Where(d => d.Date.Month == month && d.Date.Year == year)
                .ToListAsync();
            
            // Get all advances for the month
            var advances = await _context.Advances
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .ToListAsync();
            
            var result = new AllEmployeesSalaryResultDto
            {
                Month = month,
                Year = year,
                MonthName = ArabicMonthNames[month],
                WorkingDaysInMonth = workingDaysInMonth,
                HolidaysInMonth = holidaysInMonth,
                Employees = new List<EmployeeSalaryResultDto>()
            };
            
            foreach (var employee in employees)
            {
                var empAttendances = attendances.Where(a => a.Employee_id == employee.Id).ToList();
                var empBonuses = bonuses.Where(b => b.Employee_id == employee.Id).ToList();
                var empDeductions = deductions.Where(d => d.Employee_id == employee.Id).ToList();
                var empAdvances = advances.Where(a => a.Employee_id == employee.Id).ToList();
                
                // Calculate shift hours per day
                decimal shiftHoursPerDay = employee.Shift != null 
                    ? (decimal)(employee.Shift.End_time - employee.Shift.Start_time).TotalHours 
                    : 8m;
                
                // Calculate salary rates
                decimal salaryPerHour = CalculateSalaryPerHour(employee.Salary, workingDaysInMonth, shiftHoursPerDay);
                
                // Attendance calculations
                int presentDays = empAttendances.Count(a => !a.Is_Absent);
                // Absent Days = Actual Days in Month - Holidays - Present Days
                int actualDaysInMonth = DateTime.DaysInMonth(year, month);
                int absentDays = actualDaysInMonth - holidaysInMonth - presentDays;
                if (absentDays < 0) absentDays = 0; // Ensure non-negative
                
                decimal totalWorkedMinutes = empAttendances.Sum(a => a.Worked_minutes);
                decimal totalOvertimeMinutes = empAttendances.Where(a => a.OverTime != null).Sum(a => a.OverTime!.Minutes);
                decimal totalLateMinutes = empAttendances.Where(a => a.LateTime != null).Sum(a => a.LateTime!.Minutes);
                decimal totalPermissionMinutes = empAttendances.Sum(a => a.Permission_time);
                
                // Calculate overtime/latetime amounts using multipliers
                var (overtimeAmount, lateTimeDeduction, netTimeDiffAmount) = CalculateTimeDifferenceAmount(
                    totalOvertimeMinutes,
                    totalLateMinutes,
                    salaryPerHour,
                    employee.Rate_overtime_multiplier,
                    employee.Rate_latetime_multiplier);
                
                // Financial calculations
                decimal totalBonusesAmount = empBonuses.Sum(b => b.Amount);
                decimal totalDeductionsAmount = empDeductions.Sum(d => d.Amount);
                decimal totalAdvancesAmount = empAdvances.Sum(a => a.Amount);
                
                // Calculate worked hours salary
                decimal totalWorkedHours = totalWorkedMinutes / 60m;
                decimal workedHoursSalary = Math.Round(salaryPerHour * totalWorkedHours, 2);
                
                // Net Salary = (SalaryPerHour × TotalWorkedHours) + OvertimeSalary - LessTimeSalary - Deductions - Advances + Bonuses
                decimal netSalary = workedHoursSalary 
                    + overtimeAmount 
                    - lateTimeDeduction 
                    - totalDeductionsAmount 
                    - totalAdvancesAmount 
                    + totalBonusesAmount;
                
                // Gross salary = WorkedHoursSalary + Overtime + Bonuses
                decimal grossSalary = workedHoursSalary + overtimeAmount + totalBonusesAmount;
                
                // Total deductions = LateTime + Deductions + Advances
                decimal totalDeductionsCalc = lateTimeDeduction + totalDeductionsAmount + totalAdvancesAmount;
                
                var empSalary = new EmployeeSalaryResultDto
                {
                    // Employee Info
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Emp_name,
                    EmployeeCode = employee.Code,
                    DepartmentName = employee.Department?.Department_name,
                    ShiftName = employee.Shift?.Shift_name,
                    
                    // Base Salary Info
                    BaseSalary = employee.Salary,
                    SalaryPerHour = salaryPerHour,
                    
                    // Working Hours Info
                    ShiftHoursPerDay = shiftHoursPerDay,
                    ExpectedWorkingHours = workingDaysInMonth * shiftHoursPerDay,
                    
                    // Attendance Summary
                    ActualPresentDays = presentDays,
                    AbsentDays = absentDays,
                    ActualWorkedMinutes = totalWorkedMinutes,
                    ActualWorkedHours = totalWorkedMinutes / 60m,
                    
                    // Overtime Details
                    OvertimeMinutes = totalOvertimeMinutes,
                    OvertimeHours = totalOvertimeMinutes / 60m,
                    OvertimeMultiplier = employee.Rate_overtime_multiplier,
                    OvertimeAmount = overtimeAmount,
                    
                    // Late Time Details
                    LateTimeMinutes = totalLateMinutes,
                    LateTimeHours = totalLateMinutes / 60m,
                    LateTimeMultiplier = employee.Rate_latetime_multiplier,
                    LateTimeDeduction = lateTimeDeduction,
                    
                    // Net Time Difference
                    NetTimeDifferenceHours = (totalOvertimeMinutes - totalLateMinutes) / 60m,
                    NetTimeDifferenceAmount = netTimeDiffAmount,
                    
                    // Permission
                    PermissionMinutes = totalPermissionMinutes,
                    PermissionHours = totalPermissionMinutes / 60m,
                    
                    // Financial Summary
                    TotalBonuses = totalBonusesAmount,
                    TotalDeductions = totalDeductionsAmount,
                    TotalAdvances = totalAdvancesAmount,
                    
                    // Bonuses List
                    BonusesList = empBonuses.Select(b => new FinancialItemDto
                    {
                        Id = b.Id,
                        Date = b.Date,
                        Amount = b.Amount,
                        Reason = b.Reason
                    }).ToList(),
                    
                    // Deductions List
                    DeductionsList = empDeductions.Select(d => new FinancialItemDto
                    {
                        Id = d.Id,
                        Date = d.Date,
                        Amount = d.Amount,
                        Reason = d.Reason
                    }).ToList(),
                    
                    // Advances List
                    AdvancesList = empAdvances.Select(a => new FinancialItemDto
                    {
                        Id = a.Id,
                        Date = a.Date,
                        Amount = a.Amount,
                        Reason = null
                    }).ToList(),
                    
                    // Worked Hours Salary
                    WorkedHoursSalary = workedHoursSalary,
                    
                    // Final Calculations
                    GrossSalary = grossSalary,
                    TotalDeductionsAmount = totalDeductionsCalc,
                    NetSalary = netSalary
                };
                
                result.Employees.Add(empSalary);
            }
            
            // Calculate totals
            result.TotalEmployeesCount = result.Employees.Count;
            result.TotalBaseSalaries = result.Employees.Sum(e => e.BaseSalary);
            result.TotalNetSalaries = result.Employees.Sum(e => e.NetSalary);
            result.TotalBonuses = result.Employees.Sum(e => e.TotalBonuses);
            result.TotalDeductions = result.Employees.Sum(e => e.TotalDeductions);
            result.TotalAdvances = result.Employees.Sum(e => e.TotalAdvances);
            result.TotalOvertimeAmount = result.Employees.Sum(e => e.OvertimeAmount);
            result.TotalLateTimeDeduction = result.Employees.Sum(e => e.LateTimeDeduction);
            
            // Total Hours
            result.TotalWorkedHours = result.Employees.Sum(e => e.ActualWorkedHours);
            result.TotalOvertimeHours = result.Employees.Sum(e => e.OvertimeHours);
            result.TotalLateTimeHours = result.Employees.Sum(e => e.LateTimeHours);
            
            return result;
        }

        #endregion

        #region Helper Methods

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

        public decimal CalculateSalaryPerDay(decimal monthlySalary, int workingDays)
        {
            if (workingDays <= 0) return 0;
            return Math.Round(monthlySalary / workingDays, 2);
        }

        public decimal CalculateSalaryPerHour(decimal monthlySalary, int workingDays, decimal hoursPerDay)
        {
            if (workingDays <= 0 || hoursPerDay <= 0) return 0;
            return Math.Round(monthlySalary / (workingDays * hoursPerDay), 2);
        }

        /// <summary>
        /// Calculate overtime/latetime amounts using multiplier rates
        /// 
        /// Logic:
        /// - If both multipliers are 1: 
        ///   netHours = overtimeHours - lateTimeHours
        ///   if positive: amount = netHours * salaryPerHour * overtimeMultiplier
        ///   if negative: amount = netHours * salaryPerHour * lateTimeMultiplier (negative value)
        /// <summary>
        /// Calculate overtime/latetime amounts using multiplier rates
        /// 
        /// Logic:
        /// - If ANY multiplier is 1 (one or both): 
        ///   netHours = overtimeHours - lateTimeHours
        ///   if positive: amount = netHours * salaryPerHour * overtimeMultiplier
        ///   if negative: amount = netHours * salaryPerHour * lateTimeMultiplier (negative value)
        ///   
        /// - If BOTH multipliers are NOT 1 (both differ from 1):
        ///   overtimeAmount = overtimeHours * salaryPerHour * overtimeMultiplier
        ///   lateTimeDeduction = lateTimeHours * salaryPerHour * lateTimeMultiplier
        ///   net = overtimeAmount - lateTimeDeduction
        /// </summary>
        public (decimal overtimeAmount, decimal lateTimeDeduction, decimal netAmount) CalculateTimeDifferenceAmount(
            decimal overtimeMinutes, 
            decimal lateTimeMinutes, 
            decimal salaryPerHour,
            decimal overtimeMultiplier,
            decimal lateTimeMultiplier)
        {
            decimal overtimeHours = overtimeMinutes / 60m;
            decimal lateTimeHours = lateTimeMinutes / 60m;
            
            // If ANY multiplier is 1, use net difference calculation
            if (overtimeMultiplier == 1m || lateTimeMultiplier == 1m)
            {
                decimal netHours = overtimeHours - lateTimeHours;
                decimal netAmount;
                decimal overtimeAmount = 0;
                decimal lateDeduction = 0;
                
                if (netHours >= 0)
                {
                    // Positive = overtime, use overtime multiplier
                    netAmount = Math.Round(netHours * salaryPerHour * overtimeMultiplier, 2);
                    overtimeAmount = netAmount;
                }
                else
                {
                    // Negative = late/less time, use latetime multiplier
                    netAmount = Math.Round(netHours * salaryPerHour * lateTimeMultiplier, 2);
                    lateDeduction = Math.Abs(netAmount);
                }
                
                return (overtimeAmount, lateDeduction, netAmount);
            }
            else
            {
                // BOTH multipliers differ from 1, calculate separately
                decimal overtimeAmount = Math.Round(overtimeHours * salaryPerHour * overtimeMultiplier, 2);
                decimal lateDeduction = Math.Round(lateTimeHours * salaryPerHour * lateTimeMultiplier, 2);
                decimal netAmount = overtimeAmount - lateDeduction;
                
                return (overtimeAmount, lateDeduction, netAmount);
            }
        }

        #endregion

        #region PayRoll Save/Get Methods

        public async Task<PayRollExistsDto> PayRollExistsAsync(int month, int year)
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

        public async Task<SavePayRollResponseDto> SavePayRollAsync(SavePayRollRequestDto request)
        {
            var response = new SavePayRollResponseDto();
            
            try
            {
                // First calculate all salaries
                var salaryResult = await CalculateAllEmployeesSalariesAsync(
                    request.Month, 
                    request.Year, 
                    request.WorkingDaysInMonth, 
                    request.HolidaysInMonth);
                
                int savedCount = 0;
                int updatedCount = 0;
                
                foreach (var empSalary in salaryResult.Employees)
                {
                    // Get paid salary from request (or default to net salary)
                    var empRequest = request.Employees.FirstOrDefault(e => e.EmployeeId == empSalary.EmployeeId);
                    decimal paidSalary = empRequest?.PaidSalary ?? Math.Round(empSalary.NetSalary);
                    bool isPaid = empRequest?.IsPaid ?? false;
                    
                    // Check if record exists
                    var existingPayRoll = await _context.PayRolls
                        .FirstOrDefaultAsync(p => p.Employee_id == empSalary.EmployeeId 
                            && p.Month == request.Month 
                            && p.Year == request.Year);
                    
                    if (existingPayRoll != null)
                    {
                        // Update existing record
                        UpdatePayRollRecord(existingPayRoll, empSalary, request, paidSalary, isPaid);
                        updatedCount++;
                    }
                    else
                    {
                        // Create new record
                        var newPayRoll = CreatePayRollRecord(empSalary, request, paidSalary, isPaid);
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

        private PayRoll CreatePayRollRecord(EmployeeSalaryResultDto empSalary, SavePayRollRequestDto request, decimal paidSalary, bool isPaid)
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

        private void UpdatePayRollRecord(PayRoll payRoll, EmployeeSalaryResultDto empSalary, SavePayRollRequestDto request, decimal paidSalary, bool isPaid)
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

        public async Task<SavedMonthlyPayRollDto?> GetSavedPayRollAsync(int month, int year)
        {
            var records = await _context.PayRolls
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();

            if (!records.Any()) return null;

            var result = new SavedMonthlyPayRollDto
            {
                Month = month,
                Year = year,
                MonthName = ArabicMonthNames[month],
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
                    BaseSalary = r.BaseSalary,
                    SalaryPerHour = r.SalaryPerHour,
                    WorkedHoursSalary = r.WorkedHoursSalary,
                    OvertimeAmount = r.OvertimeAmount,
                    LateTimeDeduction = r.LateTimeDeduction,
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

        public async Task<bool> UpdatePaidSalaryAsync(UpdatePaidSalaryDto request)
        {
            var payRoll = await _context.PayRolls.FindAsync(request.PayRollId);
            if (payRoll == null) return false;
            
            payRoll.ActualPaidAmount = request.PaidSalary;
            payRoll.IsPaid = request.IsPaid;
            payRoll.DateSaved = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMonthPayRollAsync(int month, int year)
        {
            var records = await _context.PayRolls
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
            
            if (!records.Any()) return false;
            
            _context.PayRolls.RemoveRange(records);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
