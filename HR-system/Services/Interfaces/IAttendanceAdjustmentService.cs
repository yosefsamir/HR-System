using HR_system.DTOs.AttendanceAdjustment;

namespace HR_system.Services.Interfaces
{
    public interface IAttendanceAdjustmentService
    {
        /// <summary>
        /// Get all adjustments for a specific month/year
        /// </summary>
        Task<IEnumerable<AttendanceAdjustmentDto>> GetByMonthAsync(int month, int year);

        /// <summary>
        /// Get adjustment by ID
        /// </summary>
        Task<AttendanceAdjustmentDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create or update an adjustment (upsert per employee/month/year)
        /// </summary>
        Task<AttendanceAdjustmentDto> CreateOrUpdateAsync(CreateAttendanceAdjustmentDto dto);

        /// <summary>
        /// Update an existing adjustment
        /// </summary>
        Task<AttendanceAdjustmentDto?> UpdateAsync(int id, UpdateAttendanceAdjustmentDto dto);

        /// <summary>
        /// Delete an adjustment
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Get all active employees with their adjustments for a given month
        /// </summary>
        Task<List<EmployeeWithAdjustmentDto>> GetAllActiveEmployeesWithAdjustmentsAsync(int month, int year);
    }

    public class EmployeeWithAdjustmentDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }

        // Adjustment data (null if no adjustment exists)
        public int? AdjustmentId { get; set; }
        public string? AdjustmentType { get; set; }
        public decimal? Value { get; set; }
        public string? Reason { get; set; }
    }
}
