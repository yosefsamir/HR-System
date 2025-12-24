using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Emp_name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "الراتب مطلوب")]
        [Range(0.01, 9999999.99, ErrorMessage = "الراتب يجب أن يكون أكبر من صفر")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        public int? Age { get; set; }

        // Foreign Keys
        [ForeignKey("Department")]
        public int? Department_id { get; set; }

        [Required(ErrorMessage = "الوردية مطلوبة")]
        [ForeignKey("Shift")]
        public int Shift_id { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate_overtime_multiplier { get; set; } = 1;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate_latetime_multiplier { get; set; } = 1;

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual ICollection<Advance> Advances { get; set; } = new List<Advance>();
        public virtual ICollection<Bounes> Bounes { get; set; } = new List<Bounes>();
        public virtual ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();
        public virtual ICollection<Attendence> Attendences { get; set; } = new List<Attendence>();
        public virtual ICollection<PayRoll> PayRolls { get; set; } = new List<PayRoll>();
    }
}
