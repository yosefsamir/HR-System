using HR_system.DTOs.Bounes;

namespace HR_system.Services.Interfaces
{
    public interface IBounesService
    {
        /// <summary>
        /// Get all bonuses
        /// </summary>
        Task<IEnumerable<BounesDto>> GetAllAsync();

        /// <summary>
        /// Get bonuses by employee
        /// </summary>
        Task<IEnumerable<BounesDto>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Get bonuses by date range
        /// </summary>
        Task<IEnumerable<BounesDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get bonuses by employee and month/year
        /// </summary>
        Task<IEnumerable<BounesDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year);

        /// <summary>
        /// Get a bonus by ID
        /// </summary>
        Task<BounesDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create a new bonus
        /// </summary>
        Task<BounesDto> CreateAsync(CreateBounesDto dto);

        /// <summary>
        /// Update an existing bonus
        /// </summary>
        Task<BounesDto?> UpdateAsync(int id, UpdateBounesDto dto);

        /// <summary>
        /// Delete a bonus
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Get total bonuses for an employee in a month
        /// </summary>
        Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year);
    }
}
