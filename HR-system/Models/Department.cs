using System.ComponentModel.DataAnnotations;

namespace HR_system.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Department_name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
