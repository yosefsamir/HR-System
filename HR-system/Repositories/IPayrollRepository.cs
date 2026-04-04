using HR_system.Domain.SalaryCalculation;
using HR_system.DTOs.PayRoll;
using HR_system.DTOs.Salary;
using HR_system.Models;

namespace HR_system.Repositories
{
    public interface IPayrollRepository
    {
        Task<List<Employee>> GetEmployeesWithRelatedDataAsync(List<int> employeeIds);
        Task<List<Attendence>> GetAttendanceRecordsAsync(int month, int year);
        Task<List<Bounes>> GetBonusRecordsAsync(int month, int year);
        Task<List<Deduction>> GetDeductionRecordsAsync(int month, int year);
        Task<List<Advance>> GetAdvanceRecordsAsync(int month, int year);
        Task<List<AttendanceAdjustment>> GetAttendanceAdjustmentRecordsAsync(int month, int year);
        Task<List<int>> GetEmployeesWithRecordsInMonthAsync(int month, int year);
        Task<List<MonthlyAttendance>> GetMonthlyAttendanceRecordsAsync(int month, int year);
        Task<PayRollExistsDto> CheckPayrollExistsAsync(int month, int year);
        Task<SavePayRollResponseDto> SavePayrollAsync(SavePayRollRequestDto request, List<EmployeeSalaryResultDto> employeeSalaries);
        Task<SavedMonthlyPayRollDto?> GetSavedPayrollAsync(int month, int year, int? shiftId = null, int? employeeId = null);
        Task<bool> UpdatePaidSalaryAsync(UpdatePaidSalaryDto request);
        Task<bool> UpdatePayrollNoteAsync(UpdatePayrollNoteDto request);
        Task<bool> DeleteMonthPayrollAsync(int month, int year);
        Task<RecalculateSingleEmployeeResponseDto?> RecalculateSingleEmployeeAsync(int payrollId, SalaryCalculator salaryCalculator);
    }
}