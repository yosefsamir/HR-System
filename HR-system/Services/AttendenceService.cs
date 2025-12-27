using HR_system.DTOs.Attendence;
using HR_system.Repositories;
using HR_system.Services.Interfaces;

namespace HR_system.Services
{
    /// <summary>
    /// Application service for attendance operations
    /// Orchestrates domain logic and data access
    /// </summary>
    public class AttendenceService : IAttendenceService
    {
        private readonly IAttendenceRepository _repository;

        public AttendenceService(IAttendenceRepository repository)
        {
            _repository = repository;
        }

        #region Query Methods

        public async Task<IEnumerable<AttendenceDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<AttendenceDto>> GetByEmployeeAsync(int employeeId)
        {
            return await _repository.GetByEmployeeAsync(employeeId);
        }

        public async Task<IEnumerable<AttendenceDto>> GetByDateAsync(DateTime date)
        {
            return await _repository.GetByDateAsync(date);
        }

        public async Task<IEnumerable<AttendenceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetFilteredAsync(startDate, endDate);
        }

        public async Task<IEnumerable<AttendenceDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await _repository.GetFilteredAsync(startDate, endDate, employeeId: employeeId);
        }

        public async Task<AttendenceDto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<AttendenceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            var attendances = await _repository.GetFilteredAsync(
                startDate: date,
                endDate: date,
                employeeId: employeeId);

            return attendances.FirstOrDefault();
        }

        #endregion

        #region Create

        public async Task<AttendenceDto> CreateAsync(CreateAttendenceDto dto)
        {
            // Business rule: Check if attendance already exists for this employee and date
            if (await _repository.ExistsForEmployeeAndDateAsync(dto.Employee_id, dto.Work_date))
            {
                throw new InvalidOperationException("Attendance record already exists for this employee on this date.");
            }

            return await _repository.CreateAsync(dto);
        }

        #endregion

        #region Update

        public async Task<AttendenceDto?> UpdateAsync(int id, UpdateAttendenceDto dto)
        {
            // Business rule: Check if attendance exists
            if (!await _repository.ExistsAsync(id))
            {
                return null;
            }

            return await _repository.UpdateAsync(id, dto);
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsForDateAsync(int employeeId, DateTime date)
        {
            return await _repository.ExistsForEmployeeAndDateAsync(employeeId, date);
        }

        #endregion

        #region Summary

        public async Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var summaries = await _repository.GetSummaryAsync(startDate, endDate, employeeId: employeeId);

            return summaries.FirstOrDefault() ?? throw new InvalidOperationException("Employee not found or no data available.");
        }

        public async Task<IEnumerable<AttendanceSummaryDto>> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            int? departmentId = null,
            int? employeeId = null)
        {
            return await _repository.GetSummaryAsync(startDate, endDate, departmentId, employeeId);
        }

        #endregion
    }
}
