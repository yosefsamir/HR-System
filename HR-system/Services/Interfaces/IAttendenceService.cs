using HR_system.DTOs.Attendence;

namespace HR_system.Services.Interfaces
{
    public interface IAttendenceService
    {
        /// <summary>
        /// Get all attendance records
        /// </summary>
        Task<IEnumerable<AttendenceDto>> GetAllAsync();

        /// <summary>
        /// Get attendance by employee
        /// </summary>
        Task<IEnumerable<AttendenceDto>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Get attendance by date
        /// </summary>
        Task<IEnumerable<AttendenceDto>> GetByDateAsync(DateTime date);

        /// <summary>
        /// Get attendance by date range
        /// </summary>
        Task<IEnumerable<AttendenceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get attendance by employee and month/year
        /// </summary>
        Task<IEnumerable<AttendenceDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year);

        /// <summary>
        /// Get attendance record by ID
        /// </summary>
        Task<AttendenceDto?> GetByIdAsync(int id);

        /// <summary>
        /// Get attendance record for employee on specific date
        /// </summary>
        Task<AttendenceDto?> GetByEmployeeAndDateAsync(int employeeId, DateTime date);

        /// <summary>
        /// Create attendance record with check-in and check-out
        /// Late time and overtime will be calculated automatically
        /// If absent, all times will be zero
        /// </summary>
        Task<AttendenceDto> CreateAsync(CreateAttendenceDto dto);

        /// <summary>
        /// Update attendance record
        /// Recalculates late time, overtime, and worked minutes
        /// If marked absent, all times become zero
        /// </summary>
        Task<AttendenceDto?> UpdateAsync(int id, UpdateAttendenceDto dto);

        /// <summary>
        /// Delete an attendance record
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Get attendance summary for an employee in a month
        /// </summary>
        Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int month, int year);

        /// <summary>
        /// Check if attendance record exists for employee on date
        /// </summary>
        Task<bool> ExistsForDateAsync(int employeeId, DateTime date);
    }
}
