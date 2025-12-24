using HR_system.Data;
using HR_system.DTOs.Deduction;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class DeductionService : IDeductionService
    {
        private readonly ApplicationDbContext _context;

        public DeductionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DeductionDto>> GetAllAsync()
        {
            return await _context.Deductions
                .Include(d => d.Employee)
                .OrderByDescending(d => d.Date)
                .Select(d => new DeductionDto
                {
                    Id = d.Id,
                    Employee_id = d.Employee_id,
                    Employee_name = d.Employee != null ? d.Employee.Emp_name : string.Empty,
                    Employee_code = d.Employee != null ? d.Employee.Code : string.Empty,
                    Date = d.Date,
                    Amount = d.Amount,
                    Reason = d.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DeductionDto>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.Deductions
                .Include(d => d.Employee)
                .Where(d => d.Employee_id == employeeId)
                .OrderByDescending(d => d.Date)
                .Select(d => new DeductionDto
                {
                    Id = d.Id,
                    Employee_id = d.Employee_id,
                    Employee_name = d.Employee != null ? d.Employee.Emp_name : string.Empty,
                    Employee_code = d.Employee != null ? d.Employee.Code : string.Empty,
                    Date = d.Date,
                    Amount = d.Amount,
                    Reason = d.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DeductionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Deductions
                .Include(d => d.Employee)
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .OrderByDescending(d => d.Date)
                .Select(d => new DeductionDto
                {
                    Id = d.Id,
                    Employee_id = d.Employee_id,
                    Employee_name = d.Employee != null ? d.Employee.Emp_name : string.Empty,
                    Employee_code = d.Employee != null ? d.Employee.Code : string.Empty,
                    Date = d.Date,
                    Amount = d.Amount,
                    Reason = d.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DeductionDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Deductions
                .Include(d => d.Employee)
                .Where(d => d.Employee_id == employeeId && 
                           d.Date.Month == month && 
                           d.Date.Year == year)
                .OrderByDescending(d => d.Date)
                .Select(d => new DeductionDto
                {
                    Id = d.Id,
                    Employee_id = d.Employee_id,
                    Employee_name = d.Employee != null ? d.Employee.Emp_name : string.Empty,
                    Employee_code = d.Employee != null ? d.Employee.Code : string.Empty,
                    Date = d.Date,
                    Amount = d.Amount,
                    Reason = d.Reason
                })
                .ToListAsync();
        }

        public async Task<DeductionDto?> GetByIdAsync(int id)
        {
            var deduction = await _context.Deductions
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deduction == null)
                return null;

            return new DeductionDto
            {
                Id = deduction.Id,
                Employee_id = deduction.Employee_id,
                Employee_name = deduction.Employee?.Emp_name ?? string.Empty,
                Employee_code = deduction.Employee?.Code ?? string.Empty,
                Date = deduction.Date,
                Amount = deduction.Amount,
                Reason = deduction.Reason
            };
        }

        public async Task<DeductionDto> CreateAsync(CreateDeductionDto dto)
        {
            var deduction = new Deduction
            {
                Employee_id = dto.Employee_id,
                Date = dto.Date,
                Amount = dto.Amount,
                Reason = dto.Reason
            };

            _context.Deductions.Add(deduction);
            await _context.SaveChangesAsync();

            // Reload with employee data
            await _context.Entry(deduction).Reference(d => d.Employee).LoadAsync();

            return new DeductionDto
            {
                Id = deduction.Id,
                Employee_id = deduction.Employee_id,
                Employee_name = deduction.Employee?.Emp_name ?? string.Empty,
                Employee_code = deduction.Employee?.Code ?? string.Empty,
                Date = deduction.Date,
                Amount = deduction.Amount,
                Reason = deduction.Reason
            };
        }

        public async Task<DeductionDto?> UpdateAsync(int id, UpdateDeductionDto dto)
        {
            var deduction = await _context.Deductions
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deduction == null)
                return null;

            deduction.Date = dto.Date;
            deduction.Amount = dto.Amount;
            deduction.Reason = dto.Reason;

            await _context.SaveChangesAsync();

            return new DeductionDto
            {
                Id = deduction.Id,
                Employee_id = deduction.Employee_id,
                Employee_name = deduction.Employee?.Emp_name ?? string.Empty,
                Employee_code = deduction.Employee?.Code ?? string.Empty,
                Date = deduction.Date,
                Amount = deduction.Amount,
                Reason = deduction.Reason
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deduction = await _context.Deductions.FindAsync(id);

            if (deduction == null)
                return false;

            _context.Deductions.Remove(deduction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Deductions
                .Where(d => d.Employee_id == employeeId && 
                           d.Date.Month == month && 
                           d.Date.Year == year)
                .SumAsync(d => d.Amount);
        }
    }
}
