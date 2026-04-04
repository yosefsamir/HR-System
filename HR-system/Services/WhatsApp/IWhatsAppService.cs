namespace HR_system.Services
{
    public interface IWhatsAppService
    {
        Task<bool> StartSessionAsync();
        Task<bool> RestartSessionAsync();
        Task<bool> LogoutSessionAsync();
        Task<string?> GetQRCodeAsync();
        Task<WhatsAppSessionInfo?> GetSessionInfoAsync();
        Task<string> GetSessionStatusAsync();
        Task<WhatsAppOperationResult> SendMessageAsync(string phoneNumber, string message);
        Task<WhatsAppOperationResult> SendFileAsync(string phoneNumber, string fileName, string mimeType, byte[] fileContent, string? caption = null);
    }
}