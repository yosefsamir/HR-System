using HR_system.DTOs.MonthlyAttendance;
using HR_system.Repositories;
using HR_system.Services.Interfaces;

namespace HR_system.Services
{
    public class MonthlyAttendanceService : IMonthlyAttendanceService
    {
        private readonly IMonthlyAttendanceRepository _repository;

        public MonthlyAttendanceService(IMonthlyAttendanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MonthlyAttendanceDto>> GetByMonthAsync(int month, int year)
        {
            return await _repository.GetByMonthAsync(month, year);
        }

        public async Task<MonthlyAttendanceDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<MonthlyAttendanceDto> CreateAsync(CreateMonthlyAttendanceDto dto)
        {
            // Check uniqueness
            if (await _repository.ExistsForEmployeeMonthAsync(dto.Employee_id, dto.Month, dto.Year))
            {
                throw new InvalidOperationException("يوجد سجل حضور شهري لهذا الموظف في هذا الشهر بالفعل");
            }

            // Manual entry — set IsManuallyEntered = true
            return await _repository.CreateAsync(dto, isManuallyEntered: true);
        }

        public async Task<MonthlyAttendanceDto?> UpdateAsync(int id, CreateMonthlyAttendanceDto dto)
        {
            // When user updates, mark as manually entered
            return await _repository.UpdateAsync(id, dto, isManuallyEntered: true);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
