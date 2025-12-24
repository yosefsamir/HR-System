namespace HR_system.DTOs.PayRoll
{
    /// <summary>
    /// Request to save all payroll data for a month
    /// </summary>
    public class SavePayRollRequestDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int WorkingDaysInMonth { get; set; }
        public int HolidaysInMonth { get; set; }
        public List<SavePayRollEmployeeDto> Employees { get; set; } = new();
    }

    /// <summary>
    /// Employee data for saving payroll (only paid salary needed, rest is calculated)
    /// </summary>
    public class SavePayRollEmployeeDto
    {
        public int EmployeeId { get; set; }
        public decimal PaidSalary { get; set; }
        public bool IsPaid { get; set; }
    }

    /// <summary>
    /// Response after saving payroll
    /// </summary>
    public class SavePayRollResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SavedCount { get; set; }
        public int UpdatedCount { get; set; }
    }

    /// <summary>
    /// Request to update paid salary for single employee
    /// </summary>
    public class UpdatePaidSalaryDto
    {
        public int PayRollId { get; set; }
        public decimal PaidSalary { get; set; }
        public bool IsPaid { get; set; }
    }

    /// <summary>
    /// Saved payroll data for display (from database)
    /// </summary>
    public class SavedPayRollDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        
        // Days
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        
        // Hours (stored as decimal for precision)
        public decimal ActualWorkedMinutes { get; set; }
        public decimal ActualWorkedHours { get; set; }
        public decimal OvertimeMinutes { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeMultiplier { get; set; }
        public decimal LateTimeMinutes { get; set; }
        public decimal LateTimeHours { get; set; }
        public decimal LateTimeMultiplier { get; set; }
        
        // Hours Display
        public string ActualWorkedHours_Display => FormatHours(ActualWorkedMinutes);
        public string OvertimeHours_Display => FormatHours(OvertimeMinutes);
        public string LateTimeHours_Display => FormatHours(LateTimeMinutes);
        
        // Salary components
        public decimal BaseSalary { get; set; }
        public decimal SalaryPerHour { get; set; }
        public decimal WorkedHoursSalary { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal LateTimeDeduction { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Deductions { get; set; }
        public decimal Advances { get; set; }
        
        // Final Calculations
        public decimal GrossSalary { get; set; }
        public decimal TotalDeductionsAmount { get; set; }
        public decimal NetSalary { get; set; }
        public decimal PaidSalary { get; set; }
        public bool IsPaid { get; set; }
        
        public DateTime DateSaved { get; set; }
        
        private static string FormatHours(decimal totalMinutes)
        {
            int hours = (int)(totalMinutes / 60m);
            int minutes = Math.Abs((int)(totalMinutes % 60m));
            return $"{hours}:{minutes:D2}";
        }
    }

    /// <summary>
    /// Monthly payroll summary for saved data
    /// </summary>
    public class SavedMonthlyPayRollDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int WorkingDays { get; set; }
        public int Holidays { get; set; }
        public DateTime? DateSaved { get; set; }
        
        public int TotalEmployees { get; set; }
        public decimal TotalNetSalaries { get; set; }
        public decimal TotalPaidSalaries { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalAdvances { get; set; }
        public decimal TotalOvertimeAmount { get; set; }
        public decimal TotalLateTimeDeduction { get; set; }
        
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
        
        public List<SavedPayRollDto> Employees { get; set; } = new();
    }

    /// <summary>
    /// Simple check response
    /// </summary>
    public class PayRollExistsDto
    {
        public bool Exists { get; set; }
        public int RecordCount { get; set; }
        public DateTime? DateSaved { get; set; }
    }
}
