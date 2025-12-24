namespace HR_system.DTOs.Employee
{
    /// <summary>
    /// Detailed employee information including related data
    /// </summary>
    public class EmployeeDetailDto
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
        
        // Shift details
        public TimeSpan? Shift_Start_time { get; set; }
        public TimeSpan? Shift_End_time { get; set; }
        public decimal? Shift_StandardHours { get; set; }
        
        // Rates
        public decimal Rate_overtime_multiplier { get; set; }
        public decimal Rate_latetime_multiplier { get; set; }

        // Formatted values for display
        public string Salary_Display => Salary.ToString("N2");
        public string Shift_Time_Display => Shift_Start_time.HasValue && Shift_End_time.HasValue 
            ? $"{Shift_Start_time.Value:hh\\:mm} - {Shift_End_time.Value:hh\\:mm}" 
            : "N/A";
    }
}
