namespace HR_system.DTOs.Settings
{
    /// <summary>
    /// DTO for reading and updating application settings
    /// </summary>
    public class SettingsDto
    {
        public string CompanyName { get; set; } = "شركتي";
        public int SlipFontSize { get; set; } = 12;
        public decimal SlipWidthPercent { get; set; } = 48m;
        public string? SlipFooterMessage { get; set; }
    }
}
