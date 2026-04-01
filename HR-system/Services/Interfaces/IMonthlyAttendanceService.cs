using HR_system.DTOs.MonthlyAttendance;

namespace HR_system.Services.Interfaces
{
    public interface IMonthlyAttendanceService
    {
        Task<List<MonthlyAttendanceDto>> GetByMonthAsync(int month, int year);
        Task<MonthlyAttendanceDto?> GetByIdAsync(int id);
        Task<MonthlyAttendanceDto> CreateAsync(CreateMonthlyAttendanceDto dto);
        Task<MonthlyAttendanceDto?> UpdateAsync(int id, CreateMonthlyAttendanceDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
