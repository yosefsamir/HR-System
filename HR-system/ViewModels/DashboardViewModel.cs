namespace HR_system.ViewModels
{
    public class DashboardViewModel
    {
        // Summary Stats
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalShifts { get; set; }
        
        // Today's Attendance
        public int TodayPresent { get; set; }
        public int TodayAbsent { get; set; }
        public int TodayLate { get; set; }
        public int TodayOnTime { get; set; }
        public double TodayAttendanceRate { get; set; }
        
        // Monthly Stats (Current Month)
        public int MonthTotalWorkDays { get; set; }
        public int MonthTotalAttendance { get; set; }
        public int MonthTotalAbsences { get; set; }
        public int MonthTotalLateMinutes { get; set; }
        public int MonthTotalOvertimeMinutes { get; set; }
        
        // Recent Activity
        public List<RecentAttendanceItem> RecentAttendance { get; set; } = new();
        public List<RecentEmployeeItem> RecentEmployees { get; set; } = new();
        
        // Department Distribution
        public List<DepartmentStat> DepartmentStats { get; set; } = new();
        
        // Alerts
        public List<AlertItem> Alerts { get; set; } = new();
        
        // Financial Summary (This Month)
        public decimal TotalBonuses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalAdvances { get; set; }
        
        // Display Properties
        public string MonthTotalLateDisplay => $"{MonthTotalLateMinutes / 60}h {MonthTotalLateMinutes % 60}m";
        public string MonthTotalOvertimeDisplay => $"{MonthTotalOvertimeMinutes / 60}h {MonthTotalOvertimeMinutes % 60}m";
    }
    
    public class RecentAttendanceItem
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string CheckIn { get; set; } = "-";
        public string CheckOut { get; set; } = "-";
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = "secondary";
    }
    
    public class RecentEmployeeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Department { get; set; } = "-";
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = "secondary";
    }
    
    public class DepartmentStat
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public int PresentToday { get; set; }
        public double AttendanceRate { get; set; }
    }
    
    public class AlertItem
    {
        public string Type { get; set; } = "info"; // info, warning, danger, success
        public string Icon { get; set; } = "bi-info-circle";
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = "#";
        public DateTime Date { get; set; }
    }
}
