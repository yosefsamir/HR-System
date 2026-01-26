using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class Attendence
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int Employee_id { get; set; }

        public DateTime Work_date { get; set; }

        public TimeSpan? Check_In_time { get; set; }

        public TimeSpan? Check_out_time { get; set; }

        /// <summary>
        /// Actual worked minutes (standard work time only, excludes overtime, minus permission time)
        /// </summary>
        public int Worked_minutes { get; set; }

        public bool Is_Absent { get; set; }

        /// <summary>
        /// Permission time in minutes (leave during work hours)
        /// </summary>
        public int Permission_time { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual OverTime? OverTime { get; set; }
        public virtual LateTime? LateTime { get; set; }
        public virtual EarlyDeparture? EarlyDeparture { get; set; }
    }
}
