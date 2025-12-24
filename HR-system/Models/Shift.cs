using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int Minutes_allow_departure { get; set; }  = 0;

        [Column(TypeName = "decimal(4,2)")]
        public decimal StandardHours { get; set; }

        // Navigation property
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
