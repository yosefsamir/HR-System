using HR_system.Data;
using HR_system.DTOs.AttendanceAdjustment;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class AttendanceAdjustmentService : IAttendanceAdjustmentService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceAdjustmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AttendanceAdjustmentDto>> GetByMonthAsync(int month, int year)
        {
            return await _context.AttendanceAdjustments
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .Where(a => a.Month == month && a.Year == year)
                .OrderBy(a => a.Employee!.Emp_name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<AttendanceAdjustmentDto?> GetByIdAsync(int id)
        {
            var adjustment = await _context.AttendanceAdjustments
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adjustment == null) return null;

            return MapToDto(adjustment);
        }

        public async Task<AttendanceAdjustmentDto> CreateOrUpdateAsync(CreateAttendanceAdjustmentDto dto)
        {
            // Check if an adjustment already exists for this employee/month/year
            var existing = await _context.AttendanceAdjustments
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(a => a.Employee_id == dto.Employee_id
                    && a.Month == dto.Month
                    && a.Year == dto.Year);

            if (existing != null)
            {
                // Update existing
                existing.AdjustmentType = dto.AdjustmentType;
                existing.Value = dto.Value;
                existing.Reason = dto.Reason;
                existing.CreatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return MapToDto(existing);
            }

            // Create new
            var adjustment = new AttendanceAdjustment
            {
                Employee_id = dto.Employee_id,
                Month = dto.Month,
                Year = dto.Year,
                AdjustmentType = dto.AdjustmentType,
                Value = dto.Value,
                Reason = dto.Reason,
                CreatedAt = DateTime.Now
            };

            _context.AttendanceAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();

            // Reload with employee data
            await _context.Entry(adjustment).Reference(a => a.Employee).LoadAsync();
            if (adjustment.Employee != null)
            {
                await _context.Entry(adjustment.Employee).Reference(e => e.Department).LoadAsync();
            }

            return MapToDto(adjustment);
        }

        public async Task<AttendanceAdjustmentDto?> UpdateAsync(int id, UpdateAttendanceAdjustmentDto dto)
        {
            var adjustment = await _context.AttendanceAdjustments
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adjustment == null) return null;

            adjustment.AdjustmentType = dto.AdjustmentType;
            adjustment.Value = dto.Value;
            adjustment.Reason = dto.Reason;
            adjustment.CreatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return MapToDto(adjustment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var adjustment = await _context.AttendanceAdjustments.FindAsync(id);
            if (adjustment == null) return false;

            _context.AttendanceAdjustments.Remove(adjustment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<EmployeeWithAdjustmentDto>> GetAllActiveEmployeesWithAdjustmentsAsync(int month, int year)
        {
            // Get all active employees
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.Emp_name)
                .ToListAsync();

            // Get all adjustments for this month
            var adjustments = await _context.AttendanceAdjustments
                .Where(a => a.Month == month && a.Year == year)
                .ToListAsync();

            // Merge
            var result = employees.Select(emp =>
            {
                var adj = adjustments.FirstOrDefault(a => a.Employee_id == emp.Id);
                return new EmployeeWithAdjustmentDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Emp_name,
                    EmployeeCode = emp.Code,
                    DepartmentName = emp.Department?.Department_name,
                    AdjustmentId = adj?.Id,
                    AdjustmentType = adj?.AdjustmentType,
                    Value = adj?.Value,
                    Reason = adj?.Reason
                };
            }).ToList();

            return result;
        }

        private static AttendanceAdjustmentDto MapToDto(AttendanceAdjustment a)
        {
            return new AttendanceAdjustmentDto
            {
                Id = a.Id,
                Employee_id = a.Employee_id,
                Employee_name = a.Employee?.Emp_name ?? string.Empty,
                Employee_code = a.Employee?.Code ?? string.Empty,
                Department_name = a.Employee?.Department?.Department_name,
                Month = a.Month,
                Year = a.Year,
                AdjustmentType = a.AdjustmentType,
                Value = a.Value,
                Reason = a.Reason,
                CreatedAt = a.CreatedAt
            };
        }
    }
}
