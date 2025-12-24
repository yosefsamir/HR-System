using HR_system.DTOs.Advance;

namespace HR_system.Services.Interfaces
{
    public interface IAdvanceService
    {
        /// <summary>
        /// Get all advances
        /// </summary>
        Task<IEnumerable<AdvanceDto>> GetAllAsync();

        /// <summary>
        /// Get advances by employee
        /// </summary>
        Task<IEnumerable<AdvanceDto>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Get advances by date range
        /// </summary>
        Task<IEnumerable<AdvanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get advances by employee and month/year
        /// </summary>
        Task<IEnumerable<AdvanceDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year);

        /// <summary>
        /// Get an advance by ID
        /// </summary>
        Task<AdvanceDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create a new advance
        /// </summary>
        Task<AdvanceDto> CreateAsync(CreateAdvanceDto dto);

        /// <summary>
        /// Update an existing advance
        /// </summary>
        Task<AdvanceDto?> UpdateAsync(int id, UpdateAdvanceDto dto);

        /// <summary>
        /// Delete an advance
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Get total advances for an employee in a month
        /// </summary>
        Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year);
    }
}
