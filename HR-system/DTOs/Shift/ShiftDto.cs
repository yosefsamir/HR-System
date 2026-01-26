using HR_system.Models.Enums;

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
        public bool IsFlexible { get; set; }
        public SalaryCalculationType SalaryCalculationType { get; set; }
        public decimal EarlyDepartureMultiplier { get; set; }
        public int EmployeeCount { get; set; }

        // Formatted times for display
        public string Start_time_Display => Start_time.ToString(@"hh\:mm");
        public string End_time_Display => End_time.ToString(@"hh\:mm");
        public string StandardHours_Display => StandardHours.ToString("0.##") + " hours";
        
        // Salary calculation type display
        public string SalaryCalculationType_Display => SalaryCalculationType switch
        {
            SalaryCalculationType.Hourly => "بالساعة",
            SalaryCalculationType.Daily => "باليوم",
            _ => "غير محدد"
        };

        public string SalaryCalculationType_Description => SalaryCalculationType switch
        {
            SalaryCalculationType.Hourly => "الراتب = ساعات العمل × أجر الساعة",
            SalaryCalculationType.Daily => "الراتب = أيام الحضور × أجر اليوم",
            _ => ""
        };
    }
}
