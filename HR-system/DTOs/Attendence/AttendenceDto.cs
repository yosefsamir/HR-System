namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// Used for displaying attendance data
    /// </summary>
    public class AttendenceDto
    {
        public int Id { get; set; }
        public int Employee_id { get; set; }
        public string Employee_name { get; set; } = string.Empty;
        public string Employee_code { get; set; } = string.Empty;
        public DateTime Work_date { get; set; }
        public TimeSpan? Check_In_time { get; set; }
        public TimeSpan? Check_out_time { get; set; }
        public int Worked_minutes { get; set; }
        public bool Is_Absent { get; set; }
        public int Permission_time { get; set; }
        
        // Related data
        public int? LateTime_minutes { get; set; }
        public int? OverTime_minutes { get; set; }
        
        // Shift info
        public int? Shift_id { get; set; }
        public string? Shift_name { get; set; }
        public TimeSpan? Shift_Start_time { get; set; }
        public TimeSpan? Shift_End_time { get; set; }
        
        // Department info
        public int? Department_id { get; set; }
        public string? Department_name { get; set; }

        // Formatted values for display
        public string Work_date_Display => Work_date.ToString("yyyy/MM/dd");
        public string Check_In_Display => Check_In_time?.ToString(@"hh\:mm") ?? "-";
        public string Check_Out_Display => Check_out_time?.ToString(@"hh\:mm") ?? "-";
        public string Worked_hours_Display => $"{Worked_minutes / 60}h {Worked_minutes % 60}m";
        public string Permission_Display => Permission_time > 0 ? $"{Permission_time / 60}h {Permission_time % 60}m" : "-";
        public string LateTime_Display => LateTime_minutes.HasValue && LateTime_minutes > 0 ? $"{LateTime_minutes / 60}h {LateTime_minutes % 60}m" : "-";
        public string OverTime_Display => OverTime_minutes.HasValue && OverTime_minutes > 0 ? $"{OverTime_minutes / 60}h {OverTime_minutes % 60}m" : "-";
        public string Status_Display => Is_Absent ? "Absent" : (Check_out_time.HasValue ? "Complete" : (Check_In_time.HasValue ? "Checked In" : "Not Started"));
    }
}
