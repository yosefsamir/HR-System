namespace HR_system.DTOs.Bounes
{
    /// <summary>
    /// Used for displaying bonus data
    /// </summary>
    public class BounesDto
    {
        public int Id { get; set; }
        public int Employee_id { get; set; }
        public string Employee_name { get; set; } = string.Empty;
        public string Employee_code { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }

        // Formatted values for display
        public string Date_Display => Date.ToString("yyyy-MM-dd");
        public string Amount_Display => Amount.ToString("N2");
    }
}
