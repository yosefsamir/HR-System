using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    /// <summary>
    /// Monthly attendance summary per employee.
    /// Auto-populated from daily Attendence records during salary calculation,
    /// but manually-entered records (IsManuallyEntered = true) are never overwritten.
    /// </summary>
    public class MonthlyAttendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int Employee_id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        /// <summary>
        /// Number of days the employee was present
        /// </summary>
        public int PresentDays { get; set; }

        /// <summary>
        /// Number of days the employee was absent
        /// </summary>
        public int AbsentDays { get; set; }

        /// <summary>
        /// Total standard worked minutes for the month (excludes overtime)
        /// </summary>
        public int WorkedMinutes { get; set; }

        /// <summary>
        /// Total late arrival minutes for the month
        /// </summary>
        public int LateMinutes { get; set; }

        /// <summary>
        /// Total overtime minutes for the month
        /// </summary>
        public int OvertimeMinutes { get; set; }

        /// <summary>
        /// Total early departure minutes for the month
        /// </summary>
        public int EarlyDepartureMinutes { get; set; }

        /// <summary>
        /// Total permission minutes for the month
        /// </summary>
        public int PermissionMinutes { get; set; }

        /// <summary>
        /// true = user manually entered this data (will NOT be overwritten during salary calc)
        /// false = auto-generated from daily records (WILL be overwritten during salary calc)
        /// </summary>
        public bool IsManuallyEntered { get; set; } = false;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}
