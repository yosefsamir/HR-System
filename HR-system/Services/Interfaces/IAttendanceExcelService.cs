using HR_system.DTOs.Attendence;

namespace HR_system.Services.Interfaces
{
    /// <summary>
    /// Service for handling attendance Excel operations (import/export)
    /// </summary>
    public interface IAttendanceExcelService
    {
        /// <summary>
        /// Generate an Excel template with all active employees and their shift data
        /// </summary>
        /// <param name="date">The date for the template</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> GenerateTemplateAsync(DateTime date);

        /// <summary>
        /// Import attendance records from an Excel file
        /// </summary>
        /// <param name="fileStream">The Excel file stream</param>
        /// <returns>Import result with success/failure details</returns>
        Task<AttendanceImportResultDto> ImportFromExcelAsync(Stream fileStream);
    }
}
