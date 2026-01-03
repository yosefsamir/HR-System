namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// DTO for importing attendance records from Excel
    /// </summary>
    public class AttendanceImportDto
    {
        /// <summary>
        /// Employee Code (read from Excel)
        /// </summary>
        public string Employee_Code { get; set; } = string.Empty;

        /// <summary>
        /// Work date for the attendance record
        /// </summary>
        public DateTime Work_date { get; set; }

        /// <summary>
        /// Actual check-in time
        /// </summary>
        public TimeSpan? Check_In_time { get; set; }

        /// <summary>
        /// Actual check-out time
        /// </summary>
        public TimeSpan? Check_out_time { get; set; }

        /// <summary>
        /// Is the employee absent
        /// </summary>
        public bool Is_absent { get; set; } = false;

        /// <summary>
        /// Permission time in minutes
        /// </summary>
        public int Permission_minutes { get; set; } = 0;
    }

    /// <summary>
    /// Result of importing attendance records
    /// </summary>
    public class AttendanceImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<AttendenceDto> ImportedRecords { get; set; } = new List<AttendenceDto>();
    }
}
