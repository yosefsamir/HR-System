using System.ComponentModel.DataAnnotations;

namespace HR_system.Models
{
    /// <summary>
    /// Application settings stored in database
    /// </summary>
    public class AppSettings
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Company name displayed on salary slips
        /// </summary>
        [MaxLength(200)]
        public string CompanyName { get; set; } = "شركتي";

        /// <summary>
        /// Font size for salary slip printing (in pixels)
        /// </summary>
        public int SlipFontSize { get; set; } = 12;

        /// <summary>
        /// Slip width percentage (e.g., 48 for 48%)
        /// </summary>
        public decimal SlipWidthPercent { get; set; } = 48m;

        /// <summary>
        /// Custom footer message displayed at bottom of each slip
        /// </summary>
        [MaxLength(500)]
        public string? SlipFooterMessage { get; set; }

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
