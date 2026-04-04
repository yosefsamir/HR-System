using HR_system.DTOs.PayRoll;

namespace HR_system.Services.Interfaces
{
    public interface IPayRollWhatsAppService
    {
        Task<SendSalaryWhatsAppResultDto> SendSalaryWhatsAppAsync(int payRollId);
    }
}
