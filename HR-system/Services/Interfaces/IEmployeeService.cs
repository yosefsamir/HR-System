using HR_system.DTOs.Employee;

namespace HR_system.Services.Interfaces
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Get all employees
        /// </summary>
        Task<IEnumerable<EmployeeDto>> GetAllAsync();

        /// <summary>
        /// Get employees by department
        /// </summary>
        Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId);

        /// <summary>
        /// Get employees by shift
        /// </summary>
        Task<IEnumerable<EmployeeDto>> GetByShiftAsync(int shiftId);

        /// <summary>
        /// Get an employee by ID with full details
        /// </summary>
        Task<EmployeeDetailDto?> GetByIdAsync(int id);

        /// <summary>
        /// Get an employee by code
        /// </summary>
        Task<EmployeeDto?> GetByCodeAsync(string code);

        /// <summary>
        /// Create a new employee
        /// </summary>
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);

        /// <summary>
        /// Update an existing employee
        /// </summary>
        Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto);

        /// <summary>
        /// Delete an employee
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Check if an employee exists
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Check if employee code is unique
        /// </summary>
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);

        /// <summary>
        /// Search employees by name or code
        /// </summary>
        Task<IEnumerable<EmployeeDto>> SearchAsync(string searchTerm);
    }
}
