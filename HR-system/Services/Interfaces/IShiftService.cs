using HR_system.DTOs.Shift;

namespace HR_system.Services.Interfaces
{
    public interface IShiftService
    {
        /// <summary>
        /// Get all shifts
        /// </summary>
        Task<IEnumerable<ShiftDto>> GetAllAsync();

        /// <summary>
        /// Get a shift by ID
        /// </summary>
        Task<ShiftDto?> GetByIdAsync(int id);

        /// <summary>
        /// Create a new shift
        /// </summary>
        Task<ShiftDto> CreateAsync(CreateShiftDto dto);

        /// <summary>
        /// Update an existing shift
        /// </summary>
        Task<ShiftDto?> UpdateAsync(int id, UpdateShiftDto dto);

        /// <summary>
        /// Delete a shift
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Check if a shift exists
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Check if shift name is unique
        /// </summary>
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
