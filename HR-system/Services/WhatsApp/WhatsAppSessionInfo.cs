namespace HR_system.Services
{
    public class WhatsAppSessionInfo
    {
        public string SessionName { get; set; } = string.Empty;
        public string Status { get; set; } = "UNKNOWN";
        public string? WhatsAppId { get; set; }
        public string? PushName { get; set; }
        public string? Engine { get; set; }
    }
}
