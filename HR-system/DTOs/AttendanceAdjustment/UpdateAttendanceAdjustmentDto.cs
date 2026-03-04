using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.AttendanceAdjustment
{
    public class UpdateAttendanceAdjustmentDto
    {
        [Required]
        [StringLength(10)]
        public string AdjustmentType { get; set; } = "Days";

        [Required]
        public decimal Value { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
