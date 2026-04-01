namespace HR_system.DTOs.MonthlyAttendance
{
    /// <summary>
    /// DTO for displaying monthly attendance records
    /// </summary>
    public class MonthlyAttendanceDto
    {
        public int Id { get; set; }
        public int Employee_id { get; set; }
        public string Employee_name { get; set; } = string.Empty;
        public string Employee_code { get; set; } = string.Empty;
        public string? Department_name { get; set; }
        public string? Shift_name { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
        public int EarlyDepartureMinutes { get; set; }
        public int PermissionMinutes { get; set; }

        public bool IsManuallyEntered { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Formatted display helpers
        public string WorkedHours_Display => FormatHoursMinutes(WorkedMinutes);
        public string LateHours_Display => FormatHoursMinutes(LateMinutes);
        public string OvertimeHours_Display => FormatHoursMinutes(OvertimeMinutes);
        public string EarlyDepartureHours_Display => FormatHoursMinutes(EarlyDepartureMinutes);
        public string PermissionHours_Display => FormatHoursMinutes(PermissionMinutes);

        private static string FormatHoursMinutes(int totalMinutes)
        {
            return $"{totalMinutes / 60}h {totalMinutes % 60}m";
        }
    }
}
