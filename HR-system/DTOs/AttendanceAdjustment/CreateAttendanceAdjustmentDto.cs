using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.AttendanceAdjustment
{
    public class CreateAttendanceAdjustmentDto
    {
        [Required]
        public int Employee_id { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        [StringLength(10)]
        public string AdjustmentType { get; set; } = "Days";

        [Required]
        public decimal Value { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
