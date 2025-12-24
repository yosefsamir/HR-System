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
        
        /// <summary>
        /// Get list of employee IDs who have any records in the specified month
        /// </summary>
        Task<List<int>> GetEmployeesWithRecordsInMonthAsync(int month, int year);
        
        /// <summary>
        /// Calculate salary per hour based on monthly salary and working days
        /// </summary>
        decimal CalculateSalaryPerHour(decimal monthlySalary, int workingDays, decimal hoursPerDay);
        
        /// <summary>
        /// Calculate salary per day based on monthly salary and working days
        /// </summary>
        decimal CalculateSalaryPerDay(decimal monthlySalary, int workingDays);
        
        /// <summary>
        /// Calculate overtime/latetime amount using multiplier rates
        /// Logic:
        /// - If both multipliers are 1: (overtime - latetime) * rate
        /// - If different: (overtime * overtimeMultiplier) - (latetime * lateTimeMultiplier)
        /// </summary>
        /// <param name="overtimeMinutes">Total overtime minutes</param>
        /// <param name="lateTimeMinutes">Total late time minutes</param>
        /// <param name="salaryPerHour">Hourly rate</param>
        /// <param name="overtimeMultiplier">Overtime multiplier (e.g., 1.5)</param>
        /// <param name="lateTimeMultiplier">Late time multiplier (e.g., 1.0)</param>
        /// <returns>Tuple: (overtimeAmount, lateTimeDeduction, netAmount)</returns>
        (decimal overtimeAmount, decimal lateTimeDeduction, decimal netAmount) CalculateTimeDifferenceAmount(
            decimal overtimeMinutes, 
            decimal lateTimeMinutes, 
            decimal salaryPerHour,
            decimal overtimeMultiplier,
            decimal lateTimeMultiplier);

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
