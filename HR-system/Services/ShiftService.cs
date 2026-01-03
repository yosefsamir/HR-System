using HR_system.Data;
using HR_system.DTOs.Shift;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class ShiftService : IShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShiftDto>> GetAllAsync()
        {
            return await _context.Shifts
                .Select(s => new ShiftDto
                {
                    Id = s.Id,
                    Shift_name = s.Shift_name,
                    Start_time = s.Start_time,
                    End_time = s.End_time,
                    Minutes_allow_attendence = s.Minutes_allow_attendence,
                    Minutes_allow_departure = s.Minutes_allow_departure,
                    StandardHours = s.StandardHours,
                    IsFlexible = s.IsFlexible,
                    EmployeeCount = s.Employees.Count
                })
                .ToListAsync();
        }

        public async Task<ShiftDto?> GetByIdAsync(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.Employees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return null;

            return new ShiftDto
            {
                Id = shift.Id,
                Shift_name = shift.Shift_name,
                Start_time = shift.Start_time,
                End_time = shift.End_time,
                Minutes_allow_attendence = shift.Minutes_allow_attendence,
                Minutes_allow_departure = shift.Minutes_allow_departure,
                StandardHours = shift.StandardHours,
                IsFlexible = shift.IsFlexible,
                EmployeeCount = shift.Employees.Count
            };
        }

        public async Task<ShiftDto> CreateAsync(CreateShiftDto dto)
        {
            var shift = new Shift
            {
                Shift_name = dto.Shift_name,
                Start_time = dto.Start_time,
                End_time = dto.End_time,
                Minutes_allow_attendence = dto.Minutes_allow_attendence,
                Minutes_allow_departure = dto.Minutes_allow_departure,
                StandardHours = dto.StandardHours,
                IsFlexible = dto.IsFlexible
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return new ShiftDto
            {
                Id = shift.Id,
                Shift_name = shift.Shift_name,
                Start_time = shift.Start_time,
                End_time = shift.End_time,
                Minutes_allow_attendence = shift.Minutes_allow_attendence,
                Minutes_allow_departure = shift.Minutes_allow_departure,
                StandardHours = shift.StandardHours,
                IsFlexible = shift.IsFlexible,
                EmployeeCount = 0
            };
        }

        public async Task<ShiftDto?> UpdateAsync(int id, UpdateShiftDto dto)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
                return null;

            shift.Shift_name = dto.Shift_name;
            shift.Start_time = dto.Start_time;
            shift.End_time = dto.End_time;
            shift.Minutes_allow_attendence = dto.Minutes_allow_attendence;
            shift.Minutes_allow_departure = dto.Minutes_allow_departure;
            shift.StandardHours = dto.StandardHours;
            shift.IsFlexible = dto.IsFlexible;

            await _context.SaveChangesAsync();

            return new ShiftDto
            {
                Id = shift.Id,
                Shift_name = shift.Shift_name,
                Start_time = shift.Start_time,
                End_time = shift.End_time,
                Minutes_allow_attendence = shift.Minutes_allow_attendence,
                Minutes_allow_departure = shift.Minutes_allow_departure,
                StandardHours = shift.StandardHours,
                IsFlexible = shift.IsFlexible,
                EmployeeCount = await _context.Employees.CountAsync(e => e.Shift_id == id)
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
                return false;

            // Check if shift has employees
            var hasEmployees = await _context.Employees.AnyAsync(e => e.Shift_id == id);
            if (hasEmployees)
            {
                throw new InvalidOperationException("Cannot delete shift with assigned employees.");
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Shifts.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _context.Shifts.Where(s => s.Shift_name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
