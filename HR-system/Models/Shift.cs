using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HR_system.Models.Enums;

namespace HR_system.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Shift_name { get; set; } = string.Empty;
        
        [Required]
        public TimeSpan Start_time { get; set; }

        [Required]
        public TimeSpan End_time { get; set; }

        public int Minutes_allow_attendence { get; set; } = 0;

        public int Minutes_allow_departure { get; set; } = 0;

        [Column(TypeName = "decimal(4,2)")]
        public decimal StandardHours { get; set; }

        public bool IsFlexible { get; set; } = false;

        /// <summary>
        /// Determines how salary is calculated for employees in this shift
        /// Hourly = Worked Hours × Salary Per Hour
        /// Daily = Days Attended × Daily Salary
        /// </summary>
        public SalaryCalculationType SalaryCalculationType { get; set; } = SalaryCalculationType.Hourly;

        /// <summary>
        /// Multiplier for early departure deduction calculation
        /// Default 1 means 1x salary per hour deduction
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal EarlyDepartureMultiplier { get; set; } = 1;

        // Navigation property
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
