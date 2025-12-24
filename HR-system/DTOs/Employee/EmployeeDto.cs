namespace HR_system.DTOs.Employee
{
    /// <summary>
    /// Used for displaying employee data in lists
    /// </summary>
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Emp_name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Status { get; set; }
        
        // Foreign Keys
        public int? Department_id { get; set; }
        public int Shift_id { get; set; }
        
        // Related data names for display
        public string? Department_name { get; set; }
        public string Shift_name { get; set; } = string.Empty;
        
        // Rates
        public decimal Rate_overtime_multiplier { get; set; }
        public decimal Rate_latetime_multiplier { get; set; }

        // Formatted salary for display
        public string Salary_Display => Salary.ToString("N2");
    }
}
