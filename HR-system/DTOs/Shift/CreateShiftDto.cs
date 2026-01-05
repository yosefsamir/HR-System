using System.ComponentModel.DataAnnotations;
using HR_system.Models.Enums;

namespace HR_system.DTOs.Shift
{
    /// <summary>
    /// Used when creating a new shift
    /// </summary>
    public class CreateShiftDto
    {
        [Required(ErrorMessage = "Shift name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Shift Name")]
        public string Shift_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan Start_time { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan End_time { get; set; }

        [Range(0, 120, ErrorMessage = "Minutes must be between 0 and 120")]
        [Display(Name = "Allowed Late Minutes (Attendance)")]
        public int Minutes_allow_attendence { get; set; } = 0;

        [Range(0, 120, ErrorMessage = "Minutes must be between 0 and 120")]
        [Display(Name = "Allowed Early Leave Minutes")]
        public int Minutes_allow_departure { get; set; } = 0;

        [Required(ErrorMessage = "Standard hours is required")]
        [Range(0.5, 24, ErrorMessage = "Standard hours must be between 0.5 and 24")]
        [Display(Name = "Standard Hours")]
        public decimal StandardHours { get; set; }

        [Display(Name = "Flexible Shift (No time restrictions)")]
        public bool IsFlexible { get; set; } = false;

        [Display(Name = "Salary Calculation Type")]
        public SalaryCalculationType SalaryCalculationType { get; set; } = SalaryCalculationType.Hourly;
    }
}
