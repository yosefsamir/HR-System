namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// Monthly attendance summary for an employee
    /// </summary>
    public class AttendanceSummaryDto
    {
        // Employee Info
        public int Employee_id { get; set; }
        public string Employee_name { get; set; } = string.Empty;
        public string Employee_code { get; set; } = string.Empty;
        public int? Department_id { get; set; }
        public string? Department_name { get; set; }
        public int? Shift_id { get; set; }
        public string? Shift_name { get; set; }
        public decimal Salary { get; set; }
        
        // Period Info
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        
        // Working Days Summary
        public int TotalWorkingDays { get; set; }  // Days in month (excluding weekends if applicable)
        public int TotalPresentDays { get; set; }  // Days employee was present
        public int TotalAbsentDays { get; set; }   // Days employee was absent
        public int TotalIncompleteDays { get; set; } // Days with check-in but no check-out
        
        // Hours Summary
        public int TotalWorkedMinutes { get; set; }
        public int ExpectedWorkMinutes { get; set; }  // Based on shift hours * working days
        public int TotalLateMinutes { get; set; }
        public int TotalOvertimeMinutes { get; set; }
        public int TotalPermissionMinutes { get; set; }
        public int TotalEarlyDepartureMinutes { get; set; }
        
        // Formatted Display Values
        public string TotalWorkedHours_Display => FormatHoursMinutes(TotalWorkedMinutes);
        public string ExpectedWorkHours_Display => FormatHoursMinutes(ExpectedWorkMinutes);
        public string TotalLateHours_Display => FormatHoursMinutes(TotalLateMinutes);
        public string TotalOvertimeHours_Display => FormatHoursMinutes(TotalOvertimeMinutes);
        public string TotalPermissionHours_Display => FormatHoursMinutes(TotalPermissionMinutes);
        public string TotalEarlyDepartureHours_Display => FormatHoursMinutes(TotalEarlyDepartureMinutes);
        
        // Calculated Stats
        public decimal AttendanceRate => TotalWorkingDays > 0 
            ? Math.Round((decimal)TotalPresentDays / TotalWorkingDays * 100, 1) 
            : 0;
        
        public decimal WorkEfficiency => ExpectedWorkMinutes > 0 
            ? Math.Round((decimal)TotalWorkedMinutes / ExpectedWorkMinutes * 100, 1) 
            : 0;
        
        public int NetWorkedMinutes => TotalWorkedMinutes - TotalLateMinutes + TotalOvertimeMinutes;
        public string NetWorkedHours_Display => FormatHoursMinutes(NetWorkedMinutes);
        
        // Average per day
        public int AverageWorkedMinutesPerDay => TotalPresentDays > 0 
            ? TotalWorkedMinutes / TotalPresentDays 
            : 0;
        public string AverageWorkedHoursPerDay_Display => FormatHoursMinutes(AverageWorkedMinutesPerDay);
        
        public int AverageLateMinutesPerDay => TotalPresentDays > 0 
            ? TotalLateMinutes / TotalPresentDays 
            : 0;
        public string AverageLateMinutesPerDay_Display => FormatHoursMinutes(AverageLateMinutesPerDay);
        
        // Count statistics
        public int LateDaysCount { get; set; }      // Number of days employee was late
        public int OvertimeDaysCount { get; set; }  // Number of days with overtime
        public int EarlyLeaveDaysCount { get; set; } // Number of days employee left early
        
        // Early/On-time arrivals
        public int OnTimeArrivals { get; set; }
        public int EarlyArrivals { get; set; }
        
        // Performance indicators
        public string PerformanceStatus => GetPerformanceStatus();
        public string PerformanceBadgeClass => GetPerformanceBadgeClass();
        
        private static string FormatHoursMinutes(int totalMinutes)
        {
            if (totalMinutes < 0)
            {
                return $"-{Math.Abs(totalMinutes) / 60}h {Math.Abs(totalMinutes) % 60}m";
            }
            return $"{totalMinutes / 60}h {totalMinutes % 60}m";
        }
        
        private string GetPerformanceStatus()
        {
            if (AttendanceRate >= 95 && TotalLateMinutes < 30)
                return "ممتاز";
            if (AttendanceRate >= 85 && TotalLateMinutes < 120)
                return "جيد جداً";
            if (AttendanceRate >= 75)
                return "جيد";
            if (AttendanceRate >= 60)
                return "مقبول";
            return "ضعيف";
        }
        
        private string GetPerformanceBadgeClass()
        {
            if (AttendanceRate >= 95 && TotalLateMinutes < 30)
                return "bg-success";
            if (AttendanceRate >= 85 && TotalLateMinutes < 120)
                return "bg-info";
            if (AttendanceRate >= 75)
                return "bg-primary";
            if (AttendanceRate >= 60)
                return "bg-warning";
            return "bg-danger";
        }
    }
}
