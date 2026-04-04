using HR_system.DTOs.MonthlyAttendance;
using HR_system.Models;

namespace HR_system.Repositories
{
    public interface IMonthlyAttendanceRepository
    {
        Task<List<MonthlyAttendanceDto>> GetByMonthAsync(int month, int year);
        Task<MonthlyAttendanceDto?> GetByIdAsync(int id);
        Task<MonthlyAttendance?> GetEntityByEmployeeMonthAsync(int employeeId, int month, int year);
        Task<MonthlyAttendanceDto> CreateAsync(CreateMonthlyAttendanceDto dto, bool isManuallyEntered = true);
        Task<MonthlyAttendanceDto?> UpdateAsync(int id, CreateMonthlyAttendanceDto dto, bool isManuallyEntered = true);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsForEmployeeMonthAsync(int employeeId, int month, int year);
        Task PopulateFromDailyRecordsAsync(int month, int year, List<int> employeeIds);
        Task<List<MonthlyAttendance>> GetEntitiesByMonthAsync(int month, int year);
    }
}