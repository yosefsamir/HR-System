using HR_system.DTOs.Settings;

namespace HR_system.Services.Interfaces
{
    public interface IWhatsAppSettingsService
    {
        Task<WhatsAppActionResultDto> SendTestPdfAsync(TestWhatsAppPdfDto dto);
    }
}