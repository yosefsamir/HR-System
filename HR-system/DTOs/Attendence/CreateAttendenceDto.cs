using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// Used for creating/editing attendance with check-in and check-out together
    /// </summary>
    public class CreateAttendenceDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int Employee_id { get; set; }

        [Required(ErrorMessage = "Work date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Work Date")]
        public DateTime Work_date { get; set; } = DateTime.Today;

        [Display(Name = "Is Absent")]
        public bool Is_absent { get; set; } = false;

        [DataType(DataType.Time)]
        [Display(Name = "Check-In Time")]
        public TimeSpan? Check_In_time { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Check-Out Time")]
        public TimeSpan? Check_out_time { get; set; }

        [Range(0, 480, ErrorMessage = "Permission time must be between 0 and 480 minutes")]
        [Display(Name = "Permission Time (minutes)")]
        public int Permission_minutes { get; set; } = 0;
    }
}
