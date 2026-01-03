using HR_system.DTOs.Employee;

namespace HR_system.Services.Interfaces
{
    public interface IEmployeeExcelService
    {
        /// <summary>
        /// Export employees to Excel file
        /// </summary>
        /// <param name="employees">Employees to export</param>
        /// <returns>Excel file bytes</returns>
        byte[] ExportToExcel(IEnumerable<EmployeeDto> employees);

        /// <summary>
        /// Import employees from Excel file
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="updateExisting">Whether to update existing employees by code</param>
        /// <returns>Import result with success/failure counts and errors</returns>
        Task<EmployeeImportResult> ImportFromExcelAsync(Stream fileStream, bool updateExisting = false);

        /// <summary>
        /// Get Excel template for employee import
        /// </summary>
        /// <returns>Excel template file bytes</returns>
        byte[] GetImportTemplate();

        /// <summary>
        /// Validate Excel file structure before import
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>List of validation errors, empty if valid</returns>
        List<string> ValidateExcelStructure(Stream fileStream);
    }
}
