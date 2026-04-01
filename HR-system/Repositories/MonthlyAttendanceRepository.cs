using HR_system.Data;
using HR_system.DTOs.MonthlyAttendance;
using HR_system.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Repositories
{
    public class MonthlyAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public MonthlyAttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MonthlyAttendanceDto>> GetByMonthAsync(int month, int year)
        {
            return await _context.MonthlyAttendances
                .Include(m => m.Employee)
                .ThenInclude(e => e!.Department)
                .Include(m => m.Employee)
                .ThenInclude(e => e!.Shift)
                .Where(m => m.Month == month && m.Year == year)
                .Select(m => MapToDto(m))
                .ToListAsync();
        }

        public async Task<MonthlyAttendanceDto?> GetByIdAsync(int id)
        {
            var record = await _context.MonthlyAttendances
                .Include(m => m.Employee)
                .ThenInclude(e => e!.Department)
                .Include(m => m.Employee)
                .ThenInclude(e => e!.Shift)
                .FirstOrDefaultAsync(m => m.Id == id);

            return record != null ? MapToDto(record) : null;
        }

        public async Task<MonthlyAttendance?> GetEntityByEmployeeMonthAsync(int employeeId, int month, int year)
        {
            return await _context.MonthlyAttendances
                .FirstOrDefaultAsync(m => m.Employee_id == employeeId && m.Month == month && m.Year == year);
        }

        public async Task<MonthlyAttendanceDto> CreateAsync(CreateMonthlyAttendanceDto dto, bool isManuallyEntered = true)
        {
            var entity = new MonthlyAttendance
            {
                Employee_id = dto.Employee_id,
                Month = dto.Month,
                Year = dto.Year,
                PresentDays = dto.PresentDays,
                AbsentDays = dto.AbsentDays,
                WorkedMinutes = dto.WorkedMinutes,
                LateMinutes = dto.LateMinutes,
                OvertimeMinutes = dto.OvertimeMinutes,
                EarlyDepartureMinutes = dto.EarlyDepartureMinutes,
                PermissionMinutes = dto.PermissionMinutes,
                IsManuallyEntered = isManuallyEntered,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now
            };

            _context.MonthlyAttendances.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with includes
            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<MonthlyAttendanceDto?> UpdateAsync(int id, CreateMonthlyAttendanceDto dto, bool isManuallyEntered = true)
        {
            var entity = await _context.MonthlyAttendances.FindAsync(id);
            if (entity == null) return null;

            entity.PresentDays = dto.PresentDays;
            entity.AbsentDays = dto.AbsentDays;
            entity.WorkedMinutes = dto.WorkedMinutes;
            entity.LateMinutes = dto.LateMinutes;
            entity.OvertimeMinutes = dto.OvertimeMinutes;
            entity.EarlyDepartureMinutes = dto.EarlyDepartureMinutes;
            entity.PermissionMinutes = dto.PermissionMinutes;
            entity.IsManuallyEntered = isManuallyEntered;
            entity.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.MonthlyAttendances.FindAsync(id);
            if (entity == null) return false;

            _context.MonthlyAttendances.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsForEmployeeMonthAsync(int employeeId, int month, int year)
        {
            return await _context.MonthlyAttendances
                .AnyAsync(m => m.Employee_id == employeeId && m.Month == month && m.Year == year);
        }

        /// <summary>
        /// Auto-populate monthly attendance from daily records for all given employees.
        /// Does NOT overwrite records where IsManuallyEntered = true.
        /// </summary>
        public async Task PopulateFromDailyRecordsAsync(int month, int year, List<int> employeeIds)
        {
            foreach (var employeeId in employeeIds)
            {
                // Check if manually-entered record exists — skip it
                var existing = await _context.MonthlyAttendances
                    .FirstOrDefaultAsync(m => m.Employee_id == employeeId && m.Month == month && m.Year == year);

                if (existing != null && existing.IsManuallyEntered)
                    continue;

                // Aggregate from daily attendance records
                var dailyRecords = await _context.Attendences
                    .Include(a => a.OverTime)
                    .Include(a => a.LateTime)
                    .Include(a => a.EarlyDeparture)
                    .Where(a => a.Employee_id == employeeId && a.Work_date.Month == month && a.Work_date.Year == year)
                    .ToListAsync();

                if (!dailyRecords.Any())
                {
                    // No daily records — if there's an auto-generated record, remove it
                    if (existing != null && !existing.IsManuallyEntered)
                    {
                        _context.MonthlyAttendances.Remove(existing);
                    }
                    continue;
                }

                int presentDays = dailyRecords.Count(a => !a.Is_Absent);
                int absentDays = dailyRecords.Count(a => a.Is_Absent);
                int workedMinutes = dailyRecords.Sum(a => a.Worked_minutes);
                int lateMinutes = dailyRecords.Where(a => a.LateTime != null).Sum(a => a.LateTime!.Minutes);
                int overtimeMinutes = dailyRecords.Where(a => a.OverTime != null).Sum(a => a.OverTime!.Minutes);
                int earlyDepartureMinutes = dailyRecords.Where(a => a.EarlyDeparture != null).Sum(a => a.EarlyDeparture!.Minutes);
                int permissionMinutes = dailyRecords.Sum(a => a.Permission_time);

                if (existing != null)
                {
                    // Update existing auto-generated record
                    existing.PresentDays = presentDays;
                    existing.AbsentDays = absentDays;
                    existing.WorkedMinutes = workedMinutes;
                    existing.LateMinutes = lateMinutes;
                    existing.OvertimeMinutes = overtimeMinutes;
                    existing.EarlyDepartureMinutes = earlyDepartureMinutes;
                    existing.PermissionMinutes = permissionMinutes;
                    existing.CreatedAt = DateTime.Now;
                }
                else
                {
                    // Create new auto-generated record
                    _context.MonthlyAttendances.Add(new MonthlyAttendance
                    {
                        Employee_id = employeeId,
                        Month = month,
                        Year = year,
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        WorkedMinutes = workedMinutes,
                        LateMinutes = lateMinutes,
                        OvertimeMinutes = overtimeMinutes,
                        EarlyDepartureMinutes = earlyDepartureMinutes,
                        PermissionMinutes = permissionMinutes,
                        IsManuallyEntered = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get all monthly attendance records for a specific month/year
        /// </summary>
        public async Task<List<MonthlyAttendance>> GetEntitiesByMonthAsync(int month, int year)
        {
            return await _context.MonthlyAttendances
                .Where(m => m.Month == month && m.Year == year)
                .ToListAsync();
        }

        private static MonthlyAttendanceDto MapToDto(MonthlyAttendance m)
        {
            return new MonthlyAttendanceDto
            {
                Id = m.Id,
                Employee_id = m.Employee_id,
                Employee_name = m.Employee?.Emp_name ?? "",
                Employee_code = m.Employee?.Code ?? "",
                Department_name = m.Employee?.Department?.Department_name,
                Shift_name = m.Employee?.Shift?.Shift_name,
                Month = m.Month,
                Year = m.Year,
                PresentDays = m.PresentDays,
                AbsentDays = m.AbsentDays,
                WorkedMinutes = m.WorkedMinutes,
                LateMinutes = m.LateMinutes,
                OvertimeMinutes = m.OvertimeMinutes,
                EarlyDepartureMinutes = m.EarlyDepartureMinutes,
                PermissionMinutes = m.PermissionMinutes,
                IsManuallyEntered = m.IsManuallyEntered,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            };
        }
    }
}
