using HR_system.DTOs.Department;

namespace HR_system.Services.Interfaces
{
    public interface IDepartmentService
    {
        /// <summary>
        /// Get all departments
        /// </summary>
        Task<IEnumerable<DepartmentDto>> GetAllAsync();

        /// <summary>
        /// Get a department by ID
        /// </summary>
        Task<DepartmentDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create a new department
        /// </summary>
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);

        /// <summary>
        /// Update an existing department
        /// </summary>
        Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);

        /// <summary>
        /// Delete a department
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Check if a department exists
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Check if department name is unique
        /// </summary>
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
