using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.MonthlyAttendance
{
    /// <summary>
    /// DTO for creating/updating monthly attendance records
    /// </summary>
    public class CreateMonthlyAttendanceDto
    {
        [Required(ErrorMessage = "الموظف مطلوب")]
        public int Employee_id { get; set; }

        [Required(ErrorMessage = "الشهر مطلوب")]
        [Range(1, 12, ErrorMessage = "الشهر يجب أن يكون بين 1 و 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "السنة مطلوبة")]
        [Range(2020, 2100, ErrorMessage = "السنة غير صحيحة")]
        public int Year { get; set; }

        [Range(0, 31, ErrorMessage = "أيام الحضور يجب أن تكون بين 0 و 31")]
        public int PresentDays { get; set; }

        [Range(0, 31, ErrorMessage = "أيام الغياب يجب أن تكون بين 0 و 31")]
        public int AbsentDays { get; set; }

        [Range(0, 99999, ErrorMessage = "دقائق العمل غير صحيحة")]
        public int WorkedMinutes { get; set; }

        [Range(0, 99999, ErrorMessage = "دقائق التأخير غير صحيحة")]
        public int LateMinutes { get; set; }

        [Range(0, 99999, ErrorMessage = "دقائق الإضافي غير صحيحة")]
        public int OvertimeMinutes { get; set; }

        [Range(0, 99999, ErrorMessage = "دقائق الانصراف المبكر غير صحيحة")]
        public int EarlyDepartureMinutes { get; set; }

        [Range(0, 99999, ErrorMessage = "دقائق الاستئذان غير صحيحة")]
        public int PermissionMinutes { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
