using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class AttendanceAdjustment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int Employee_id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        /// <summary>
        /// "Days" or "Hours"
        /// </summary>
        [Required]
        [StringLength(10)]
        public string AdjustmentType { get; set; } = "Days";

        /// <summary>
        /// Positive = bonus, Negative = deduction
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}
