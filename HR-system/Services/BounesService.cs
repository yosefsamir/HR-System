using HR_system.Data;
using HR_system.DTOs.Bounes;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class BounesService : IBounesService
    {
        private readonly ApplicationDbContext _context;

        public BounesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BounesDto>> GetAllAsync()
        {
            return await _context.Bounes
                .Include(b => b.Employee)
                .OrderByDescending(b => b.Date)
                .Select(b => new BounesDto
                {
                    Id = b.Id,
                    Employee_id = b.Employee_id,
                    Employee_name = b.Employee != null ? b.Employee.Emp_name : string.Empty,
                    Employee_code = b.Employee != null ? b.Employee.Code : string.Empty,
                    Date = b.Date,
                    Amount = b.Amount,
                    Reason = b.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BounesDto>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.Bounes
                .Include(b => b.Employee)
                .Where(b => b.Employee_id == employeeId)
                .OrderByDescending(b => b.Date)
                .Select(b => new BounesDto
                {
                    Id = b.Id,
                    Employee_id = b.Employee_id,
                    Employee_name = b.Employee != null ? b.Employee.Emp_name : string.Empty,
                    Employee_code = b.Employee != null ? b.Employee.Code : string.Empty,
                    Date = b.Date,
                    Amount = b.Amount,
                    Reason = b.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BounesDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Bounes
                .Include(b => b.Employee)
                .Where(b => b.Date >= startDate && b.Date <= endDate)
                .OrderByDescending(b => b.Date)
                .Select(b => new BounesDto
                {
                    Id = b.Id,
                    Employee_id = b.Employee_id,
                    Employee_name = b.Employee != null ? b.Employee.Emp_name : string.Empty,
                    Employee_code = b.Employee != null ? b.Employee.Code : string.Empty,
                    Date = b.Date,
                    Amount = b.Amount,
                    Reason = b.Reason
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BounesDto>> GetByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Bounes
                .Include(b => b.Employee)
                .Where(b => b.Employee_id == employeeId && 
                           b.Date.Month == month && 
                           b.Date.Year == year)
                .OrderByDescending(b => b.Date)
                .Select(b => new BounesDto
                {
                    Id = b.Id,
                    Employee_id = b.Employee_id,
                    Employee_name = b.Employee != null ? b.Employee.Emp_name : string.Empty,
                    Employee_code = b.Employee != null ? b.Employee.Code : string.Empty,
                    Date = b.Date,
                    Amount = b.Amount,
                    Reason = b.Reason
                })
                .ToListAsync();
        }

        public async Task<BounesDto?> GetByIdAsync(int id)
        {
            var bonus = await _context.Bounes
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bonus == null)
                return null;

            return new BounesDto
            {
                Id = bonus.Id,
                Employee_id = bonus.Employee_id,
                Employee_name = bonus.Employee?.Emp_name ?? string.Empty,
                Employee_code = bonus.Employee?.Code ?? string.Empty,
                Date = bonus.Date,
                Amount = bonus.Amount,
                Reason = bonus.Reason
            };
        }

        public async Task<BounesDto> CreateAsync(CreateBounesDto dto)
        {
            var bonus = new Bounes
            {
                Employee_id = dto.Employee_id,
                Date = dto.Date,
                Amount = dto.Amount,
                Reason = dto.Reason
            };

            _context.Bounes.Add(bonus);
            await _context.SaveChangesAsync();

            // Reload with employee data
            await _context.Entry(bonus).Reference(b => b.Employee).LoadAsync();

            return new BounesDto
            {
                Id = bonus.Id,
                Employee_id = bonus.Employee_id,
                Employee_name = bonus.Employee?.Emp_name ?? string.Empty,
                Employee_code = bonus.Employee?.Code ?? string.Empty,
                Date = bonus.Date,
                Amount = bonus.Amount,
                Reason = bonus.Reason
            };
        }

        public async Task<BounesDto?> UpdateAsync(int id, UpdateBounesDto dto)
        {
            var bonus = await _context.Bounes
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bonus == null)
                return null;

            bonus.Date = dto.Date;
            bonus.Amount = dto.Amount;
            bonus.Reason = dto.Reason;

            await _context.SaveChangesAsync();

            return new BounesDto
            {
                Id = bonus.Id,
                Employee_id = bonus.Employee_id,
                Employee_name = bonus.Employee?.Emp_name ?? string.Empty,
                Employee_code = bonus.Employee?.Code ?? string.Empty,
                Date = bonus.Date,
                Amount = bonus.Amount,
                Reason = bonus.Reason
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bonus = await _context.Bounes.FindAsync(id);

            if (bonus == null)
                return false;

            _context.Bounes.Remove(bonus);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTotalByEmployeeAndMonthAsync(int employeeId, int month, int year)
        {
            return await _context.Bounes
                .Where(b => b.Employee_id == employeeId && 
                           b.Date.Month == month && 
                           b.Date.Year == year)
                .SumAsync(b => b.Amount);
        }
    }
}
