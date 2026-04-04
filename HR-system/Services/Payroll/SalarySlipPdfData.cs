namespace HR_system.Services.Payroll
{
    public class SalarySlipPdfData
    {
        public int PayRollId { get; set; }
        public string? CompanyName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? ShiftName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal SalaryPerHour { get; set; }
        public decimal SalaryPerDay { get; set; }
        public string SalaryCalculationTypeDisplay { get; set; } = string.Empty;
        public int WorkingDaysInMonth { get; set; }
        public int HolidaysInMonth { get; set; }
        public int ActualPresentDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal ActualWorkedMinutes { get; set; }
        public decimal ExpectedWorkingHours { get; set; }
        public decimal OvertimeMinutes { get; set; }
        public decimal OvertimeMultiplier { get; set; }
        public decimal LateTimeMinutes { get; set; }
        public decimal LateTimeMultiplier { get; set; }
        public decimal EarlyDepartureMinutes { get; set; }
        public decimal EarlyDepartureMultiplier { get; set; }
        public decimal PermissionMinutes { get; set; }
        public decimal WorkedHoursSalary { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal LateTimeDeduction { get; set; }
        public decimal EarlyDepartureDeduction { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalAdvances { get; set; }
        public decimal MonthlyFixedAllowance { get; set; }
        public decimal TotalAttendanceAdjustment { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalDeductionsAmount { get; set; }
        public decimal NetSalary { get; set; }
        public decimal ActualPaidAmount { get; set; }
        public decimal PreviousMonthCarryOver { get; set; }
        public decimal SalaryCarryOver { get; set; }
        public string? EmployeeNote { get; set; }
        public bool IsPaid { get; set; }
        public DateTime DateSaved { get; set; }
    }
}
