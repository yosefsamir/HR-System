using HR_system.DTOs.Attendence;
using HR_system.Models;

namespace HR_system.Repositories
{
    /// <summary>
    /// Repository interface for attendance data operations
    /// </summary>
    public interface IAttendenceRepository
    {
        #region Query Methods
        Task<IEnumerable<AttendenceDto>> GetAllAsync();
        Task<IEnumerable<AttendenceDto>> GetByEmployeeAsync(int employeeId);
        Task<IEnumerable<AttendenceDto>> GetByDateAsync(DateTime date);
        Task<IEnumerable<AttendenceDto>> GetFilteredAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? departmentId = null,
            int? shiftId = null,
            int? employeeId = null);
        Task<AttendenceDto?> GetByIdAsync(int id);
        #endregion

        #region Create
        Task<AttendenceDto> CreateAsync(CreateAttendenceDto dto);
        #endregion

        #region Update
        Task<AttendenceDto?> UpdateAsync(int id, UpdateAttendenceDto dto);
        #endregion

        #region Delete
        Task<bool> DeleteAsync(int id);
        #endregion

        #region Summary
        Task<IEnumerable<AttendanceSummaryDto>> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            int? departmentId = null,
            int? employeeId = null);
        #endregion

        #region Existence Checks
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForEmployeeAndDateAsync(int employeeId, DateTime date);
        #endregion
    }
}