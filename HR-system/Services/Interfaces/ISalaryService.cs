using HR_system.DTOs.Salary;
using HR_system.DTOs.PayRoll;

namespace HR_system.Services.Interfaces
{
    public interface ISalaryService
    {
        /// <summary>
        /// Calculate monthly salary for all employees who have any records in the month
        /// (attendance, bonuses, deductions, or advances)
        /// </summary>
        /// <param name="request">Calculation parameters (month, year, working days, holidays)</param>
        /// <returns>All employees salary calculation results</returns>
        Task<AllEmployeesSalaryResultDto> CalculateAllEmployeesSalariesAsync(SalaryCalculationRequestDto request);
        
        /// <summary>
        /// Calculate monthly salary for all employees with auto-detected working days
        /// </summary>
        /// <param name="month">Month (1-12)</param>
        /// <param name="year">Year</param>
        /// <param name="workingDaysInMonth">Number of working days in month</param>
        /// <param name="holidaysInMonth">Number of holidays</param>
        /// <returns>All employees salary calculation results</returns>
        Task<AllEmployeesSalaryResultDto> CalculateAllEmployeesSalariesAsync(int month, int year, int workingDaysInMonth, int holidaysInMonth = 0);

        #region PayRoll Save/Get Methods

        /// <summary>
        /// Check if payroll exists for month/year
        /// </summary>
        Task<PayRollExistsDto> PayRollExistsAsync(int month, int year);

        /// <summary>
        /// Save calculated payroll to database
        /// </summary>
        Task<SavePayRollResponseDto> SavePayRollAsync(SavePayRollRequestDto request);

        /// <summary>
        /// Get saved payroll data for month/year
        /// </summary>
        Task<SavedMonthlyPayRollDto?> GetSavedPayRollAsync(int month, int year);

        /// <summary>
        /// Update paid salary for single employee
        /// </summary>
        Task<bool> UpdatePaidSalaryAsync(UpdatePaidSalaryDto request);

        /// <summary>
        /// Delete all payroll records for month/year
        /// </summary>
        Task<bool> DeleteMonthPayRollAsync(int month, int year);

        #endregion
    }
}
