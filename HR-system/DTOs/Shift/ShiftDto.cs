namespace HR_system.DTOs.Shift
{
    /// <summary>
    /// Used for displaying shift data
    /// </summary>
    public class ShiftDto
    {
        public int Id { get; set; }
        public string Shift_name { get; set; } = string.Empty;
        public TimeSpan Start_time { get; set; }
        public TimeSpan End_time { get; set; }
        public int Minutes_allow_attendence { get; set; }
        public int Minutes_allow_departure { get; set; }
        public decimal StandardHours { get; set; }
        public int EmployeeCount { get; set; }

        // Formatted times for display
        public string Start_time_Display => Start_time.ToString(@"hh\:mm");
        public string End_time_Display => End_time.ToString(@"hh\:mm");
        public string StandardHours_Display => StandardHours.ToString("0.##") + " hours";
    }
}
