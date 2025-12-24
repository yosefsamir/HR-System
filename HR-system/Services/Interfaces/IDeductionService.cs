using HR_system.DTOs.Deduction;

namespace HR_system.Services.Interfaces
{
    public interface IDeductionService
    {
        /// <summary>
        /// Get all deductions
        /// </summary>
        Task<IEnumerable<DeductionDto>> GetAllAsync();

        /// <summary>
        /// Get deductions by employee
        /// </summary>
        Task<IEnumerable<DeductionDto>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// Get deductions by date range
        /// </summary>
        Task<IEnumerable<DeductionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get deductions by employee and month/year
        /// </summary>
        Task<IEnumerable<DeductionDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year);

        /// <summary>
        /// Get a deduction by ID
        /// </summary>
        Task<DeductionDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create a new deduction
        /// </summary>
        Task<DeductionDto> CreateAsync(CreateDeductionDto dto);

        /// <summary>
        /// Update an existing deduction
        /// </summary>
        Task<DeductionDto?> UpdateAsync(int id, UpdateDeductionDto dto);

        /// <summary>
        /// Delete a deduction
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Get total deductions for an employee in a month
        /// </summary>
        Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year);
    }
}
