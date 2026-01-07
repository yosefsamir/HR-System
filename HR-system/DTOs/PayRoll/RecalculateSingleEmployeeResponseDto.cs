namespace HR_system.DTOs.PayRoll
{
    /// <summary>
    /// Response DTO for recalculating a single employee's salary
    /// </summary>
    public class RecalculateSingleEmployeeResponseDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;

        // Attendance
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }

        // Hours
        public decimal ActualWorkedMinutes { get; set; }
        public decimal ActualWorkedHours { get; set; }
        public string ActualWorkedHours_Display => FormatHours(ActualWorkedMinutes);

        // Overtime
        public decimal OvertimeMinutes { get; set; }
        public decimal OvertimeHours { get; set; }
        public string OvertimeHours_Display => FormatHours(OvertimeMinutes);
        public decimal OvertimeMultiplier { get; set; }
        public decimal OvertimeAmount { get; set; }

        // Late Time
        public decimal LateTimeMinutes { get; set; }
        public decimal LateTimeHours { get; set; }
        public string LateTimeHours_Display => FormatHours(LateTimeMinutes);
        public decimal LateTimeMultiplier { get; set; }
        public decimal LateTimeDeduction { get; set; }

        // Salary Info
        public decimal BaseSalary { get; set; }
        public decimal SalaryPerHour { get; set; }
        public decimal SalaryPerDay { get; set; }
        public string SalaryCalculationType { get; set; } = "Hourly";
        public string SalaryCalculationTypeDisplay { get; set; } = "بالساعة";

        // Calculated Amounts
        public decimal WorkedHoursSalary { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Deductions { get; set; }
        public decimal Advances { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TotalDeductionsAmount { get; set; }
        public decimal NetSalary { get; set; }

        // Payment Status
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
}
