using HR_system.Models;
using HR_system.Models.Enums;
using HR_system.DTOs.Salary;

namespace HR_system.Domain.SalaryCalculation
{
    /// <summary>
    /// Domain class responsible for salary calculation business logic
    /// </summary>
    public class SalaryCalculator
    {
        private static readonly string[] ArabicMonthNames =
        {
            "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        };

        /// <summary>
        /// Calculate salary per hour based on monthly salary and working parameters
        /// </summary>
        public decimal CalculateSalaryPerHour(decimal monthlySalary, int workingDays, decimal hoursPerDay)
        {
            if (workingDays <= 0 || hoursPerDay <= 0) return 0;
            return Math.Round(monthlySalary / (workingDays * hoursPerDay), 2);
        }

        /// <summary>
        /// Calculate salary per day based on monthly salary and working days
        /// </summary>
        public decimal CalculateSalaryPerDay(decimal monthlySalary, int workingDays)
        {
            if (workingDays <= 0) return 0;
            return Math.Round(monthlySalary / workingDays, 2);
        }

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

        /// <summary>
        /// Calculate complete salary details for an employee
        /// </summary>
        public EmployeeSalaryResultDto CalculateEmployeeSalary(
            Employee employee,
            IEnumerable<Attendence> attendances,
            IEnumerable<Bounes> bonuses,
            IEnumerable<Deduction> deductions,
            IEnumerable<Advance> advances,
            int workingDaysInMonth,
            int holidaysInMonth,
            int year,
            int month)
        {
            // Calculate actual working days in month
            int acctualWorkingDaysInMonth = DateTime.DaysInMonth(year, month) - holidaysInMonth;
            
            // Calculate shift hours per day using StandardHours from shift
            decimal shiftHoursPerDay = employee.Shift != null
                ? employee.Shift.StandardHours
                : 8m;

            // Determine salary calculation type from shift
            SalaryCalculationType calculationType = employee.Shift?.SalaryCalculationType ?? SalaryCalculationType.Hourly;

            // Calculate salary rates
            decimal salaryPerHour = CalculateSalaryPerHour(employee.Salary, workingDaysInMonth, shiftHoursPerDay);
            decimal salaryPerDay = CalculateSalaryPerDay(employee.Salary, workingDaysInMonth);

            // Filter employee-specific records
            var empAttendances = attendances.Where(a => a.Employee_id == employee.Id).ToList();
            var empBonuses = bonuses.Where(b => b.Employee_id == employee.Id).ToList();
            var empDeductions = deductions.Where(d => d.Employee_id == employee.Id).ToList();
            var empAdvances = advances.Where(a => a.Employee_id == employee.Id).ToList();

            // Calculate attendance summary
            var attendanceSummary = CalculateAttendanceSummary(empAttendances, holidaysInMonth, year, month);

            // Calculate time differences
            var timeDifferences = CalculateTimeDifferences(empAttendances);

            // Calculate time difference amounts (using hourly rate for overtime/latetime calculations)
            var (overtimeAmount, lateTimeDeduction, netTimeDiffAmount) = CalculateTimeDifferenceAmount(
                timeDifferences.TotalOvertimeMinutes,
                timeDifferences.TotalLateMinutes,
                salaryPerHour,
                employee.Rate_overtime_multiplier,
                employee.Rate_latetime_multiplier);

            // Calculate financial summaries
            var financialSummary = CalculateFinancialSummary(empBonuses, empDeductions, empAdvances);

            // Calculate final salary amounts based on calculation type
            var salaryAmounts = CalculateSalaryAmounts(
                timeDifferences.TotalWorkedMinutes,
                salaryPerHour,
                salaryPerDay,
                attendanceSummary.PresentDays,
                calculationType,
                overtimeAmount,
                lateTimeDeduction,
                financialSummary);

            // Build result DTO
            return BuildEmployeeSalaryResult(
                employee,
                salaryPerHour,
                salaryPerDay,
                shiftHoursPerDay,
                acctualWorkingDaysInMonth,
                calculationType,
                attendanceSummary,
                timeDifferences,
                overtimeAmount,
                lateTimeDeduction,
                netTimeDiffAmount,
                financialSummary,
                salaryAmounts,
                empBonuses,
                empDeductions,
                empAdvances);
        }

        private AttendanceSummary CalculateAttendanceSummary(List<Attendence> attendances, int holidaysInMonth, int year, int month)
        {
            int presentDays = attendances.Count(a => !a.Is_Absent);
            int actualDaysInMonth = DateTime.DaysInMonth(year, month);
            // Use actual absent days from attendance records instead of calculating
            int absentDays = attendances.Count(a => a.Is_Absent);

            return new AttendanceSummary
            {
                PresentDays = presentDays,
                AbsentDays = absentDays,
                ActualDaysInMonth = actualDaysInMonth
            };
        }

        private TimeDifferences CalculateTimeDifferences(List<Attendence> attendances)
        {
            decimal totalWorkedMinutes = attendances.Sum(a => a.Worked_minutes);
            decimal totalOvertimeMinutes = attendances.Where(a => a.OverTime != null).Sum(a => a.OverTime!.Minutes);
            decimal totalLateMinutes = attendances.Where(a => a.LateTime != null).Sum(a => a.LateTime!.Minutes);
            decimal totalPermissionMinutes = attendances.Sum(a => a.Permission_time);

            return new TimeDifferences
            {
                TotalWorkedMinutes = totalWorkedMinutes,
                TotalOvertimeMinutes = totalOvertimeMinutes,
                TotalLateMinutes = totalLateMinutes,
                TotalPermissionMinutes = totalPermissionMinutes
            };
        }

        private FinancialSummary CalculateFinancialSummary(
            List<Bounes> bonuses,
            List<Deduction> deductions,
            List<Advance> advances)
        {
            return new FinancialSummary
            {
                TotalBonusesAmount = bonuses.Sum(b => b.Amount),
                TotalDeductionsAmount = deductions.Sum(d => d.Amount),
                TotalAdvancesAmount = advances.Sum(a => a.Amount)
            };
        }

        private SalaryAmounts CalculateSalaryAmounts(
            decimal totalWorkedMinutes,
            decimal salaryPerHour,
            decimal salaryPerDay,
            int presentDays,
            SalaryCalculationType calculationType,
            decimal overtimeAmount,
            decimal lateTimeDeduction,
            FinancialSummary financialSummary)
        {
            decimal baseSalary;
            decimal totalWorkedHours = totalWorkedMinutes / 60m;

            // Calculate base salary based on calculation type
            if (calculationType == SalaryCalculationType.Daily)
            {
                // Daily: Salary = Days Attended × Daily Salary
                baseSalary = Math.Round(salaryPerDay * presentDays, 2);
            }
            else
            {
                // Hourly: Salary = Worked Hours × Salary Per Hour
                baseSalary = Math.Round(salaryPerHour * totalWorkedHours, 2);
            }

            // Net Salary = BaseSalary + OvertimeSalary - LessTimeSalary - Deductions - Advances + Bonuses
            decimal netSalary = baseSalary
                + overtimeAmount
                - lateTimeDeduction
                - financialSummary.TotalDeductionsAmount
                - financialSummary.TotalAdvancesAmount
                + financialSummary.TotalBonusesAmount;

            // Gross salary = BaseSalary + Overtime + Bonuses
            decimal grossSalary = baseSalary + overtimeAmount + financialSummary.TotalBonusesAmount;

            // Total deductions = LateTime + Deductions + Advances
            decimal totalDeductionsCalc = lateTimeDeduction + financialSummary.TotalDeductionsAmount + financialSummary.TotalAdvancesAmount;

            return new SalaryAmounts
            {
                WorkedHoursSalary = baseSalary,
                NetSalary = netSalary,
                GrossSalary = grossSalary,
                TotalDeductionsAmount = totalDeductionsCalc
            };
        }

        private EmployeeSalaryResultDto BuildEmployeeSalaryResult(
            Employee employee,
            decimal salaryPerHour,
            decimal salaryPerDay,
            decimal shiftHoursPerDay,
            int acctualWorkingDaysInMonth,
            SalaryCalculationType calculationType,
            AttendanceSummary attendance,
            TimeDifferences timeDiff,
            decimal overtimeAmount,
            decimal lateTimeDeduction,
            decimal netTimeDiffAmount,
            FinancialSummary financial,
            SalaryAmounts salaryAmounts,
            List<Bounes> bonuses,
            List<Deduction> deductions,
            List<Advance> advances)
        {
            return new EmployeeSalaryResultDto
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
                SalaryPerDay = salaryPerDay,
                SalaryCalculationType = calculationType.ToString(),
                SalaryCalculationTypeDisplay = calculationType == SalaryCalculationType.Daily ? "باليوم" : "بالساعة",

                // Working Hours Info
                ShiftHoursPerDay = shiftHoursPerDay,
                ExpectedWorkingHours = acctualWorkingDaysInMonth * shiftHoursPerDay,

                // Attendance Summary
                ActualPresentDays = attendance.PresentDays,
                AbsentDays = attendance.AbsentDays,
                ActualWorkedMinutes = timeDiff.TotalWorkedMinutes,
                ActualWorkedHours = timeDiff.TotalWorkedMinutes / 60m,

                // Overtime Details
                OvertimeMinutes = timeDiff.TotalOvertimeMinutes,
                OvertimeHours = timeDiff.TotalOvertimeMinutes / 60m,
                OvertimeMultiplier = employee.Rate_overtime_multiplier,
                OvertimeAmount = overtimeAmount,

                // Late Time Details
                LateTimeMinutes = timeDiff.TotalLateMinutes,
                LateTimeHours = timeDiff.TotalLateMinutes / 60m,
                LateTimeMultiplier = employee.Rate_latetime_multiplier,
                LateTimeDeduction = lateTimeDeduction,

                // Net Time Difference
                NetTimeDifferenceHours = (timeDiff.TotalOvertimeMinutes - timeDiff.TotalLateMinutes) / 60m,
                NetTimeDifferenceAmount = netTimeDiffAmount,

                // Permission
                PermissionMinutes = timeDiff.TotalPermissionMinutes,
                PermissionHours = timeDiff.TotalPermissionMinutes / 60m,

                // Financial Summary
                TotalBonuses = financial.TotalBonusesAmount,
                TotalDeductions = financial.TotalDeductionsAmount,
                TotalAdvances = financial.TotalAdvancesAmount,

                // Bonuses List
                BonusesList = bonuses.Select(b => new FinancialItemDto
                {
                    Id = b.Id,
                    Date = b.Date,
                    Amount = b.Amount,
                    Reason = b.Reason
                }).ToList(),

                // Deductions List
                DeductionsList = deductions.Select(d => new FinancialItemDto
                {
                    Id = d.Id,
                    Date = d.Date,
                    Amount = d.Amount,
                    Reason = d.Reason
                }).ToList(),

                // Advances List
                AdvancesList = advances.Select(a => new FinancialItemDto
                {
                    Id = a.Id,
                    Date = a.Date,
                    Amount = a.Amount,
                    Reason = null
                }).ToList(),

                // Worked Hours Salary
                WorkedHoursSalary = salaryAmounts.WorkedHoursSalary,

                // Final Calculations
                GrossSalary = salaryAmounts.GrossSalary,
                TotalDeductionsAmount = salaryAmounts.TotalDeductionsAmount,
                NetSalary = salaryAmounts.NetSalary
            };
        }

        /// <summary>
        /// Calculate totals for all employees
        /// </summary>
        public void CalculateTotals(AllEmployeesSalaryResultDto result)
        {
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
        }

        // Private helper classes for internal calculations
        private class AttendanceSummary
        {
            public int PresentDays { get; set; }
            public int AbsentDays { get; set; }
            public int ActualDaysInMonth { get; set; }
        }

        private class TimeDifferences
        {
            public decimal TotalWorkedMinutes { get; set; }
            public decimal TotalOvertimeMinutes { get; set; }
            public decimal TotalLateMinutes { get; set; }
            public decimal TotalPermissionMinutes { get; set; }
        }

        private class FinancialSummary
        {
            public decimal TotalBonusesAmount { get; set; }
            public decimal TotalDeductionsAmount { get; set; }
            public decimal TotalAdvancesAmount { get; set; }
        }

        private class SalaryAmounts
        {
            public decimal WorkedHoursSalary { get; set; }
            public decimal NetSalary { get; set; }
            public decimal GrossSalary { get; set; }
            public decimal TotalDeductionsAmount { get; set; }
        }
    }
}