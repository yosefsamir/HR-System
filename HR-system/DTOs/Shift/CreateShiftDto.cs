using System.ComponentModel.DataAnnotations;

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
    }
}
