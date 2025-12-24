using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class LateTime
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Attendence")]
        public int Attendence_id { get; set; }

        public int Minutes { get; set; }

        // Navigation property
        public virtual Attendence? Attendence { get; set; }
    }
}
