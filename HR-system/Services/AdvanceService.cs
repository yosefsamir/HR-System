using HR_system.Data;
using HR_system.DTOs.Advance;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class AdvanceService : IAdvanceService
    {
        private readonly ApplicationDbContext _context;

        public AdvanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdvanceDto>> GetAllAsync()
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .Select(a => new AdvanceDto
                {
                    Id = a.Id,
                    Employee_id = a.Employee_id,
                    Employee_name = a.Employee != null ? a.Employee.Emp_name : string.Empty,
                    Employee_code = a.Employee != null ? a.Employee.Code : string.Empty,
                    Date = a.Date,
                    Amount = a.Amount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AdvanceDto>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Where(a => a.Employee_id == employeeId)
                .OrderByDescending(a => a.Date)
                .Select(a => new AdvanceDto
                {
                    Id = a.Id,
                    Employee_id = a.Employee_id,
                    Employee_name = a.Employee != null ? a.Employee.Emp_name : string.Empty,
                    Employee_code = a.Employee != null ? a.Employee.Code : string.Empty,
                    Date = a.Date,
                    Amount = a.Amount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AdvanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .Select(a => new AdvanceDto
                {
                    Id = a.Id,
                    Employee_id = a.Employee_id,
                    Employee_name = a.Employee != null ? a.Employee.Emp_name : string.Empty,
                    Employee_code = a.Employee != null ? a.Employee.Code : string.Empty,
                    Date = a.Date,
                    Amount = a.Amount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AdvanceDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Advances
                .Include(a => a.Employee)
                .Where(a => a.Employee_id == employeeId && 
                           a.Date.Month == month && 
                           a.Date.Year == year)
                .OrderByDescending(a => a.Date)
                .Select(a => new AdvanceDto
                {
                    Id = a.Id,
                    Employee_id = a.Employee_id,
                    Employee_name = a.Employee != null ? a.Employee.Emp_name : string.Empty,
                    Employee_code = a.Employee != null ? a.Employee.Code : string.Empty,
                    Date = a.Date,
                    Amount = a.Amount
                })
                .ToListAsync();
        }

        public async Task<AdvanceDto?> GetByIdAsync(int id)
        {
            var advance = await _context.Advances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advance == null)
                return null;

            return new AdvanceDto
            {
                Id = advance.Id,
                Employee_id = advance.Employee_id,
                Employee_name = advance.Employee?.Emp_name ?? string.Empty,
                Employee_code = advance.Employee?.Code ?? string.Empty,
                Date = advance.Date,
                Amount = advance.Amount
            };
        }

        public async Task<AdvanceDto> CreateAsync(CreateAdvanceDto dto)
        {
            var advance = new Advance
            {
                Employee_id = dto.Employee_id,
                Date = dto.Date,
                Amount = dto.Amount
            };

            _context.Advances.Add(advance);
            await _context.SaveChangesAsync();

            // Reload with employee data
            await _context.Entry(advance).Reference(a => a.Employee).LoadAsync();

            return new AdvanceDto
            {
                Id = advance.Id,
                Employee_id = advance.Employee_id,
                Employee_name = advance.Employee?.Emp_name ?? string.Empty,
                Employee_code = advance.Employee?.Code ?? string.Empty,
                Date = advance.Date,
                Amount = advance.Amount
            };
        }

        public async Task<AdvanceDto?> UpdateAsync(int id, UpdateAdvanceDto dto)
        {
            var advance = await _context.Advances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advance == null)
                return null;

            advance.Date = dto.Date;
            advance.Amount = dto.Amount;

            await _context.SaveChangesAsync();

            return new AdvanceDto
            {
                Id = advance.Id,
                Employee_id = advance.Employee_id,
                Employee_name = advance.Employee?.Emp_name ?? string.Empty,
                Employee_code = advance.Employee?.Code ?? string.Empty,
                Date = advance.Date,
                Amount = advance.Amount
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var advance = await _context.Advances.FindAsync(id);

            if (advance == null)
                return false;

            _context.Advances.Remove(advance);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Advances
                .Where(a => a.Employee_id == employeeId && 
                           a.Date.Month == month && 
                           a.Date.Year == year)
                .SumAsync(a => a.Amount);
        }
    }
}
