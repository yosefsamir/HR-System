namespace HR_system.DTOs.Salary
{
    /// <summary>
    /// Input parameters for salary calculation for all employees
    /// </summary>
    public class SalaryCalculationRequestDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        
        /// <summary>
        /// Total working days in the month (excluding weekends/holidays)
        /// </summary>
        public int WorkingDaysInMonth { get; set; }
        
        /// <summary>
        /// Number of official holidays in the month
        /// </summary>
        public int HolidaysInMonth { get; set; }
    }

    /// <summary>
    /// Result containing all employees salary calculations
    /// </summary>
    public class AllEmployeesSalaryResultDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int WorkingDaysInMonth { get; set; }
        public int HolidaysInMonth { get; set; }
        public decimal ExpectedWorkingHoursPerDay { get; set; }
        
        public int TotalEmployeesCount { get; set; }
        public decimal TotalBaseSalaries { get; set; }
        public decimal TotalNetSalaries { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalAdvances { get; set; }
        public decimal TotalOvertimeAmount { get; set; }
        public decimal TotalLateTimeDeduction { get; set; }
        public decimal TotalEarlyDepartureDeduction { get; set; }
        
        // Total Hours
        public decimal TotalWorkedHours { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public decimal TotalLateTimeHours { get; set; }
        public decimal TotalEarlyDepartureHours { get; set; }
        
        // Display Helpers
        public string TotalWorkedHours_Display => FormatHoursMinutes(TotalWorkedHours);
        public string TotalOvertimeHours_Display => FormatHoursMinutes(TotalOvertimeHours);
        public string TotalLateTimeHours_Display => FormatHoursMinutes(TotalLateTimeHours);
        public string TotalEarlyDepartureHours_Display => FormatHoursMinutes(TotalEarlyDepartureHours);
        
        private static string FormatHoursMinutes(decimal hours)
        {
            var h = (int)Math.Floor(hours);
            var m = (int)Math.Round((hours - h) * 60);
            return $"{h}س {m}د";
        }
        
        public List<EmployeeSalaryResultDto> Employees { get; set; } = new();
    }

    /// <summary>
    /// Detailed salary calculation result for one employee
    /// </summary>
    public class EmployeeSalaryResultDto
    {
        // Employee Info
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? ShiftName { get; set; }
        
        // Base Salary Info
        public decimal BaseSalary { get; set; }
        public decimal SalaryPerHour { get; set; }
        public decimal SalaryPerDay { get; set; }
        public string SalaryCalculationType { get; set; } = "Hourly";
        public string SalaryCalculationTypeDisplay { get; set; } = "بالساعة";
        
        // Working Hours Info
        public decimal ShiftHoursPerDay { get; set; }
        public decimal ExpectedWorkingHours { get; set; }
        
        // Attendance Summary
        public int ActualPresentDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal ActualWorkedMinutes { get; set; }
        public decimal ActualWorkedHours { get; set; }
        
        // Overtime Details
        public decimal OvertimeMinutes { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeMultiplier { get; set; }
        public decimal OvertimeAmount { get; set; }
        
        // Late Time / Less Time Details
        public decimal LateTimeMinutes { get; set; }
        public decimal LateTimeHours { get; set; }
        public decimal LateTimeMultiplier { get; set; }
        public decimal LateTimeDeduction { get; set; }
        
        // Early Departure Details
        public decimal EarlyDepartureMinutes { get; set; }
        public decimal EarlyDepartureHours { get; set; }
        public decimal EarlyDepartureMultiplier { get; set; }
        public decimal EarlyDepartureDeduction { get; set; }
        
        // Net Time Difference (Overtime - LateTime with multipliers)
        public decimal NetTimeDifferenceHours { get; set; }
        public decimal NetTimeDifferenceAmount { get; set; }
        
        // Permission Hours
        public decimal PermissionMinutes { get; set; }
        public decimal PermissionHours { get; set; }
        
        // Financial Summary
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalAdvances { get; set; }
        
        // Bonuses Breakdown
        public List<FinancialItemDto> BonusesList { get; set; } = new();
        
        // Deductions Breakdown
        public List<FinancialItemDto> DeductionsList { get; set; } = new();
        
        // Advances Breakdown
        public List<FinancialItemDto> AdvancesList { get; set; } = new();
        
        // Worked Hours Salary (SalaryPerHour × ActualWorkedHours)
        public decimal WorkedHoursSalary { get; set; }
        
        // Final Calculations
        public decimal GrossSalary { get; set; }  // WorkedHoursSalary + Overtime + Bonuses
        public decimal TotalDeductionsAmount { get; set; }  // LateTime + EarlyDeparture + Deductions + Advances
        public decimal NetSalary { get; set; }  // Gross - TotalDeductions
        
        // Display Helpers
        public string OvertimeHours_Display => FormatHoursMinutes(OvertimeHours);
        public string LateTimeHours_Display => FormatHoursMinutes(LateTimeHours);
        public string EarlyDepartureHours_Display => FormatHoursMinutes(EarlyDepartureHours);
        public string ActualWorkedHours_Display => FormatHoursMinutes(ActualWorkedHours);
        public string ExpectedWorkingHours_Display => FormatHoursMinutes(ExpectedWorkingHours);
        
        private static string FormatHoursMinutes(decimal hours)
        {
            var h = (int)Math.Floor(hours);
            var m = (int)Math.Round((hours - h) * 60);
            return $"{h}س {m}د";
        }
    }

    /// <summary>
    /// Financial item (bonus, deduction, advance)
    /// </summary>
    public class FinancialItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
    }
}
