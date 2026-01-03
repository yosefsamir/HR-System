namespace HR_system.DTOs.Employee
{
    /// <summary>
    /// DTO for importing employees from Excel
    /// </summary>
    public class EmployeeImportDto
    {
        public string Emp_name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Department_name { get; set; }
        public string? Shift_name { get; set; }
        public string? Status { get; set; }
        public decimal Rate_overtime_multiplier { get; set; } = 1.5m;
        public decimal Rate_latetime_multiplier { get; set; } = 1m;
    }

    /// <summary>
    /// DTO for exporting employees to Excel
    /// </summary>
    public class EmployeeExportDto
    {
        public string Code { get; set; } = string.Empty;
        public string Emp_name { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Department_name { get; set; }
        public string Shift_name { get; set; } = string.Empty;
        public string? Status { get; set; }
        public decimal Rate_overtime_multiplier { get; set; }
        public decimal Rate_latetime_multiplier { get; set; }
    }

    /// <summary>
    /// Result of the import operation
    /// </summary>
    public class EmployeeImportResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<EmployeeImportError> Errors { get; set; } = new();
    }

    /// <summary>
    /// Error details for a failed import row
    /// </summary>
    public class EmployeeImportError
    {
        public int RowNumber { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
