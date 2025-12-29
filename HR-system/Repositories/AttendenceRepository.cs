using HR_system.Data;
using HR_system.Domain;
using HR_system.DTOs.Attendence;
using HR_system.Models;
using HR_system.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Repositories
{
    /// <summary>
    /// Repository implementation for attendance data operations
    /// </summary>
    public class AttendenceRepository : IAttendenceRepository
    {
        private readonly ApplicationDbContext _context;

        public AttendenceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Query Methods

        public async Task<IEnumerable<AttendenceDto>> GetAllAsync()
        {
            return await _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .OrderByDescending(a => a.Work_date)
                .ThenBy(a => a.Employee!.Emp_name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendenceDto>> GetByEmployeeAsync(int employeeId)
        {
            return await _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .Where(a => a.Employee_id == employeeId)
                .OrderByDescending(a => a.Work_date)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendenceDto>> GetByDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .Where(a => a.Work_date.Date == dateOnly)
                .OrderBy(a => a.Employee!.Emp_name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendenceDto>> GetFilteredAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? departmentId = null,
            int? shiftId = null,
            int? employeeId = null)
        {
            var query = _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Work_date.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.Work_date.Date <= endDate.Value.Date);

            if (departmentId.HasValue)
                query = query.Where(a => a.Employee!.Department_id == departmentId.Value);

            if (shiftId.HasValue)
                query = query.Where(a => a.Employee!.Shift_id == shiftId.Value);

            if (employeeId.HasValue)
                query = query.Where(a => a.Employee_id == employeeId.Value);

            return await query
                .OrderByDescending(a => a.Work_date)
                .ThenBy(a => a.Employee!.Emp_name)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<AttendenceDto?> GetByIdAsync(int id)
        {
            var attendance = await _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return null;

            return MapToDto(attendance);
        }

        #endregion

        #region Create

        public async Task<AttendenceDto> CreateAsync(CreateAttendenceDto dto)
        {
            // Check if already has attendance for this date
            if (await ExistsForEmployeeAndDateAsync(dto.Employee_id, dto.Work_date))
            {
                throw new InvalidOperationException("Attendance record already exists for this employee on this date.");
            }

            // Get employee with shift info
            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.Id == dto.Employee_id);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.Shift == null)
                throw new InvalidOperationException("Employee does not have an assigned shift.");

            var shift = employee.Shift;

            // Create attendance record
            var attendance = new Attendence
            {
                Employee_id = dto.Employee_id,
                Work_date = dto.Work_date.Date,
                Is_Absent = dto.Is_absent,
                Permission_time = dto.Permission_minutes
            };

            if (dto.Is_absent)
            {
                // If absent: no check-in, no check-out, worked time = 0, no late, no overtime
                attendance.Check_In_time = null;
                attendance.Check_out_time = null;
                attendance.Worked_minutes = 0;
            }
            else
            {
                // Validate times
                if (!dto.Check_In_time.HasValue)
                    throw new InvalidOperationException("Check-in time is required when not absent.");

                if (!dto.Check_out_time.HasValue)
                    throw new InvalidOperationException("Check-out time is required when not absent.");

                attendance.Check_In_time = dto.Check_In_time;
                attendance.Check_out_time = dto.Check_out_time;

                // Calculate worked minutes based on shift type
                if (shift.IsFlexible)
                {
                    // For flexible shifts: worked minutes = actual time worked (check-out - check-in - permission)
                    var totalMinutes = (int)(dto.Check_out_time.Value - dto.Check_In_time.Value).TotalMinutes;
                    if (totalMinutes < 0)
                    {
                        totalMinutes += 24 * 60; // Handle overnight shifts
                    }
                    attendance.Worked_minutes = Math.Max(0, totalMinutes - dto.Permission_minutes);
                }
                else
                {
                    // For traditional shifts: use standard calculation
                    attendance.Worked_minutes = AttendanceCalculationService.CalculateWorkedMinutes(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift,
                        dto.Permission_minutes);
                }
            }

            _context.Attendences.Add(attendance);
            await _context.SaveChangesAsync();

            // Calculate and add late time and overtime (only if not absent)
            if (!dto.Is_absent && dto.Check_In_time.HasValue && dto.Check_out_time.HasValue)
            {
                if (shift.IsFlexible)
                {
                    // For flexible shifts: calculate based on StandardHours
                    var (overtimeMinutes, lateMinutes) = AttendanceCalculationService.CalculateFlexibleTimeDifferences(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift,
                        dto.Permission_minutes);

                    if (lateMinutes > 0)
                    {
                        var lateTime = new LateTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = lateMinutes
                        };
                        _context.LateTimes.Add(lateTime);
                    }

                    if (overtimeMinutes > 0)
                    {
                        var overtime = new OverTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = overtimeMinutes
                        };
                        _context.OverTimes.Add(overtime);
                    }
                }
                else
                {
                    // For traditional shifts: use standard calculations
                    var lateMinutes = AttendanceCalculationService.CalculateLateMinutes(dto.Check_In_time.Value, shift);
                    if (lateMinutes > 0)
                    {
                        var lateTime = new LateTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = lateMinutes
                        };
                        _context.LateTimes.Add(lateTime);
                    }

                    var totalOvertimeMinutes = AttendanceCalculationService.CalculateTotalOvertimeMinutes(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift);

                    if (totalOvertimeMinutes > 0)
                    {
                        var overtime = new OverTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = totalOvertimeMinutes
                        };
                        _context.OverTimes.Add(overtime);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return (await GetByIdAsync(attendance.Id))!;
        }

        #endregion

        #region Update

        public async Task<AttendenceDto?> UpdateAsync(int id, UpdateAttendenceDto dto)
        {
            var attendance = await _context.Attendences
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Shift)
                .Include(a => a.Employee)
                    .ThenInclude(e => e!.Department)
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return null;

            var employee = attendance.Employee;
            if (employee?.Shift == null)
                throw new InvalidOperationException("Employee does not have an assigned shift.");

            var shift = employee.Shift;

            // Remove existing late time and overtime records
            if (attendance.LateTime != null)
            {
                _context.LateTimes.Remove(attendance.LateTime);
                attendance.LateTime = null;
            }
            if (attendance.OverTime != null)
            {
                _context.OverTimes.Remove(attendance.OverTime);
                attendance.OverTime = null;
            }

            attendance.Is_Absent = dto.Is_absent;
            attendance.Work_date = dto.Work_date;
            attendance.Permission_time = dto.Permission_minutes;

            if (dto.Is_absent)
            {
                // If absent: clear times, worked = 0, no late, no overtime
                attendance.Check_In_time = null;
                attendance.Check_out_time = null;
                attendance.Worked_minutes = 0;
            }
            else
            {
                // Validate times
                if (!dto.Check_In_time.HasValue)
                    throw new InvalidOperationException("Check-in time is required when not absent.");

                if (!dto.Check_out_time.HasValue)
                    throw new InvalidOperationException("Check-out time is required when not absent.");

                attendance.Check_In_time = dto.Check_In_time;
                attendance.Check_out_time = dto.Check_out_time;

                // Calculate worked minutes based on shift type
                if (shift.IsFlexible)
                {
                    // For flexible shifts: worked minutes = actual time worked (check-out - check-in - permission)
                    var totalMinutes = (int)(dto.Check_out_time.Value - dto.Check_In_time.Value).TotalMinutes;
                    if (totalMinutes < 0)
                    {
                        totalMinutes += 24 * 60; // Handle overnight shifts
                    }
                    attendance.Worked_minutes = Math.Max(0, totalMinutes - dto.Permission_minutes);
                }
                else
                {
                    // For traditional shifts: use standard calculation
                    attendance.Worked_minutes = AttendanceCalculationService.CalculateWorkedMinutes(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift,
                        dto.Permission_minutes);
                }

                // Calculate and add late time and overtime based on shift type
                if (shift.IsFlexible)
                {
                    // For flexible shifts: calculate based on StandardHours
                    var (overtimeMinutes, lateMinutes) = AttendanceCalculationService.CalculateFlexibleTimeDifferences(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift,
                        dto.Permission_minutes);

                    if (lateMinutes > 0)
                    {
                        var lateTime = new LateTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = lateMinutes
                        };
                        _context.LateTimes.Add(lateTime);
                    }

                    if (overtimeMinutes > 0)
                    {
                        var overtime = new OverTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = overtimeMinutes
                        };
                        _context.OverTimes.Add(overtime);
                    }
                }
                else
                {
                    // For traditional shifts: use standard calculations
                    var lateMinutes = AttendanceCalculationService.CalculateLateMinutes(dto.Check_In_time.Value, shift);
                    if (lateMinutes > 0)
                    {
                        var lateTime = new LateTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = lateMinutes
                        };
                        _context.LateTimes.Add(lateTime);
                    }

                    var totalOvertimeMinutes = AttendanceCalculationService.CalculateTotalOvertimeMinutes(
                        dto.Check_In_time.Value,
                        dto.Check_out_time.Value,
                        shift);

                    if (totalOvertimeMinutes > 0)
                    {
                        var overtime = new OverTime
                        {
                            Attendence_id = attendance.Id,
                            Minutes = totalOvertimeMinutes
                        };
                        _context.OverTimes.Add(overtime);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(attendance.Id);
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _context.Attendences
                .Include(a => a.LateTime)
                .Include(a => a.OverTime)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return false;

            // Delete related records
            if (attendance.LateTime != null)
                _context.LateTimes.Remove(attendance.LateTime);

            if (attendance.OverTime != null)
                _context.OverTimes.Remove(attendance.OverTime);

            _context.Attendences.Remove(attendance);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Summary

        public async Task<IEnumerable<AttendanceSummaryDto>> GetSummaryAsync(
            DateTime startDate,
            DateTime endDate,
            int? departmentId = null,
            int? employeeId = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Include(e => e.Attendences)
                    .ThenInclude(a => a.LateTime)
                .Include(e => e.Attendences)
                    .ThenInclude(a => a.OverTime)
                .AsQueryable();

            if (departmentId.HasValue)
                query = query.Where(e => e.Department_id == departmentId.Value);

            if (employeeId.HasValue)
                query = query.Where(e => e.Id == employeeId.Value);

            var employees = await query.ToListAsync();

            var summaries = new List<AttendanceSummaryDto>();

            foreach (var employee in employees)
            {
                var attendances = employee.Attendences
                    .Where(a => a.Work_date.Date >= startDate.Date && a.Work_date.Date <= endDate.Date)
                    .ToList();

                // Calculate working days in the period (excluding Fridays)
                var workingDays = 0;
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    if (date.DayOfWeek != DayOfWeek.Friday)
                        workingDays++;
                }

                // Calculate expected work minutes based on shift
                var shiftMinutesPerDay = employee.Shift != null
                    ? (int)(employee.Shift.End_time - employee.Shift.Start_time).TotalMinutes
                    : 480; // Default 8 hours

                var presentDays = attendances.Count(a => !a.Is_Absent);
                var incompleteDays = attendances.Count(a => !a.Is_Absent && a.Check_In_time.HasValue && !a.Check_out_time.HasValue);

                // Get month name in Arabic (assuming single month for now)
                var monthNames = new[] { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                                          "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

                var summary = new AttendanceSummaryDto
                {
                    // Employee Info
                    Employee_id = employee.Id,
                    Employee_name = employee.Emp_name,
                    Employee_code = employee.Code,
                    Department_id = employee.Department_id,
                    Department_name = employee.Department?.Department_name,
                    Shift_id = employee.Shift_id,
                    Shift_name = employee.Shift?.Shift_name,
                    Salary = employee.Salary,

                    // Period Info
                    Month = startDate.Month,
                    Year = startDate.Year,
                    MonthName = monthNames[startDate.Month],

                    // Working Days Summary
                    TotalWorkingDays = workingDays,
                    TotalPresentDays = presentDays,
                    TotalAbsentDays = attendances.Count(a => a.Is_Absent),
                    TotalIncompleteDays = incompleteDays,

                    // Hours Summary
                    TotalWorkedMinutes = attendances.Sum(a => a.Worked_minutes),
                    ExpectedWorkMinutes = workingDays * shiftMinutesPerDay,
                    TotalLateMinutes = attendances.Where(a => a.LateTime != null).Sum(a => a.LateTime!.Minutes),
                    TotalOvertimeMinutes = attendances.Where(a => a.OverTime != null).Sum(a => a.OverTime!.Minutes),
                    TotalPermissionMinutes = attendances.Sum(a => a.Permission_time),

                    // Count statistics
                    LateDaysCount = attendances.Count(a => a.LateTime != null && a.LateTime.Minutes > 0),
                    OvertimeDaysCount = attendances.Count(a => a.OverTime != null && a.OverTime.Minutes > 0),
                    EarlyLeaveDaysCount = attendances.Count(a => !a.Is_Absent && a.Check_out_time.HasValue &&
                        employee.Shift != null && a.Check_out_time.Value < employee.Shift.End_time),

                    // On-time/Early arrivals
                    OnTimeArrivals = attendances.Count(a => !a.Is_Absent && a.Check_In_time.HasValue &&
                        employee.Shift != null &&
                        a.Check_In_time.Value >= employee.Shift.Start_time &&
                        a.Check_In_time.Value <= employee.Shift.Start_time.Add(TimeSpan.FromMinutes(employee.Shift.Minutes_allow_attendence))),
                    EarlyArrivals = attendances.Count(a => !a.Is_Absent && a.Check_In_time.HasValue &&
                        employee.Shift != null && a.Check_In_time.Value < employee.Shift.Start_time)
                };

                summaries.Add(summary);
            }

            return summaries;
        }

        #endregion

        #region Existence Checks

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Attendences.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> ExistsForEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            return await _context.Attendences
                .AnyAsync(a => a.Employee_id == employeeId && a.Work_date.Date == date.Date);
        }

        #endregion

        #region Mapping Helper

        private static AttendenceDto MapToDto(Attendence a)
        {
            return new AttendenceDto
            {
                Id = a.Id,
                Employee_id = a.Employee_id,
                Employee_name = a.Employee?.Emp_name ?? string.Empty,
                Employee_code = a.Employee?.Code ?? string.Empty,
                Work_date = a.Work_date,
                Check_In_time = a.Check_In_time,
                Check_out_time = a.Check_out_time,
                Worked_minutes = a.Worked_minutes,
                Is_Absent = a.Is_Absent,
                Permission_time = a.Permission_time,
                LateTime_minutes = a.LateTime?.Minutes,
                OverTime_minutes = a.OverTime?.Minutes,
                Shift_id = a.Employee?.Shift_id,
                Shift_name = a.Employee?.Shift?.Shift_name,
                Shift_Start_time = a.Employee?.Shift?.Start_time,
                Shift_End_time = a.Employee?.Shift?.End_time,
                Department_id = a.Employee?.Department_id,
                Department_name = a.Employee?.Department?.Department_name
            };
        }

        #endregion
    }
}