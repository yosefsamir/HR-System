using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    /// <summary>
    /// Tracks early departure (leaving before shift end time)
    /// </summary>
    public class EarlyDeparture
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
