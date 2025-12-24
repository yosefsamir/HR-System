using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class Bounes
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int Employee_id { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}
