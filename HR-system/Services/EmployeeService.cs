using HR_system.Data;
using HR_system.DTOs.Employee;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Emp_name = e.Emp_name,
                    Code = e.Code,
                    Salary = e.Salary,
                    Gender = e.Gender,
                    Age = e.Age,
                    Status = e.Status,
                    Department_id = e.Department_id,
                    Shift_id = e.Shift_id,
                    Department_name = e.Department != null ? e.Department.Department_name : null,
                    Shift_name = e.Shift != null ? e.Shift.Shift_name : null,
                    Rate_overtime_multiplier = e.Rate_overtime_multiplier,
                    Rate_latetime_multiplier = e.Rate_latetime_multiplier
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Where(e => e.Department_id == departmentId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Emp_name = e.Emp_name,
                    Code = e.Code,
                    Salary = e.Salary,
                    Gender = e.Gender,
                    Age = e.Age,
                    Status = e.Status,
                    Department_id = e.Department_id,
                    Shift_id = e.Shift_id,
                    Department_name = e.Department != null ? e.Department.Department_name : null,
                    Shift_name = e.Shift != null ? e.Shift.Shift_name : null,
                    Rate_overtime_multiplier = e.Rate_overtime_multiplier,
                    Rate_latetime_multiplier = e.Rate_latetime_multiplier
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeDto>> GetByShiftAsync(int shiftId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Where(e => e.Shift_id == shiftId)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Emp_name = e.Emp_name,
                    Code = e.Code,
                    Salary = e.Salary,
                    Gender = e.Gender,
                    Age = e.Age,
                    Status = e.Status,
                    Department_id = e.Department_id,
                    Shift_id = e.Shift_id,
                    Department_name = e.Department != null ? e.Department.Department_name : null,
                    Shift_name = e.Shift != null ? e.Shift.Shift_name : null,
                    Rate_overtime_multiplier = e.Rate_overtime_multiplier,
                    Rate_latetime_multiplier = e.Rate_latetime_multiplier
                })
                .ToListAsync();
        }

        public async Task<EmployeeDetailDto?> GetByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return null;

            return new EmployeeDetailDto
            {
                Id = employee.Id,
                Emp_name = employee.Emp_name,
                Code = employee.Code,
                Salary = employee.Salary,
                Gender = employee.Gender,
                Age = employee.Age,
                Status = employee.Status,
                Department_id = employee.Department_id,
                Shift_id = employee.Shift_id,
                Department_name = employee.Department?.Department_name,
                Shift_name = employee.Shift?.Shift_name,
                Shift_Start_time = employee.Shift?.Start_time,
                Shift_End_time = employee.Shift?.End_time,
                Shift_StandardHours = employee.Shift?.StandardHours,
                Rate_overtime_multiplier = employee.Rate_overtime_multiplier,
                Rate_latetime_multiplier = employee.Rate_latetime_multiplier
            };
        }

        public async Task<EmployeeDto?> GetByCodeAsync(string code)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.Code == code);

            if (employee == null)
                return null;

            return new EmployeeDto
            {
                Id = employee.Id,
                Emp_name = employee.Emp_name,
                Code = employee.Code,
                Salary = employee.Salary,
                Gender = employee.Gender,
                Age = employee.Age,
                Status = employee.Status,
                Department_id = employee.Department_id,
                Shift_id = employee.Shift_id,
                Department_name = employee.Department?.Department_name,
                Shift_name = employee.Shift?.Shift_name,
                Rate_overtime_multiplier = employee.Rate_overtime_multiplier,
                Rate_latetime_multiplier = employee.Rate_latetime_multiplier
            };
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
        {
            // Validate employee code uniqueness
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Code == dto.Code);

            if (existingEmployee != null)
            {
                throw new InvalidOperationException($"Employee code '{dto.Code}' already exists. Please use a unique code.");
            }

            // Validate foreign keys - Department is optional
            if (dto.Department_id.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.Department_id.Value);
                if (department == null)
                {
                    throw new InvalidOperationException($"Department with ID {dto.Department_id} does not exist.");
                }
            }

            // Shift is required
            var shift = await _context.Shifts.FindAsync(dto.Shift_id);
            if (shift == null)
            {
                throw new InvalidOperationException($"Shift with ID {dto.Shift_id} does not exist.");
            }

            var employee = new Employee
            {
                Emp_name = dto.Emp_name,
                Code = dto.Code,
                Salary = dto.Salary,
                Gender = dto.Gender,
                Age = dto.Age,
                Department_id = dto.Department_id,
                Shift_id = dto.Shift_id,
                Status = dto.Status,
                Rate_overtime_multiplier = dto.Rate_overtime_multiplier,
                Rate_latetime_multiplier = dto.Rate_latetime_multiplier
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Reload with related data
            await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
            await _context.Entry(employee).Reference(e => e.Shift).LoadAsync();

            return new EmployeeDto
            {
                Id = employee.Id,
                Emp_name = employee.Emp_name,
                Code = employee.Code,
                Salary = employee.Salary,
                Gender = employee.Gender,
                Age = employee.Age,
                Status = employee.Status,
                Department_id = employee.Department_id,
                Shift_id = employee.Shift_id,
                Department_name = employee.Department?.Department_name,
                Shift_name = employee.Shift?.Shift_name,
                Rate_overtime_multiplier = employee.Rate_overtime_multiplier,
                Rate_latetime_multiplier = employee.Rate_latetime_multiplier
            };
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return null;

            // Validate employee code uniqueness (exclude current employee)
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Code == dto.Code && e.Id != id);

            if (existingEmployee != null)
            {
                throw new InvalidOperationException($"Employee code '{dto.Code}' already exists. Please use a unique code.");
            }

            // Validate foreign keys - Department is optional
            if (dto.Department_id.HasValue)
            {
                var department = await _context.Departments.FindAsync(dto.Department_id.Value);
                if (department == null)
                {
                    throw new InvalidOperationException($"Department with ID {dto.Department_id} does not exist.");
                }
            }

            // Shift is required
            var shift = await _context.Shifts.FindAsync(dto.Shift_id);
            if (shift == null)
            {
                throw new InvalidOperationException($"Shift with ID {dto.Shift_id} does not exist.");
            }

            employee.Emp_name = dto.Emp_name;
            employee.Code = dto.Code;
            employee.Salary = dto.Salary;
            employee.Gender = dto.Gender;
            employee.Age = dto.Age;
            employee.Department_id = dto.Department_id;
            employee.Shift_id = dto.Shift_id;
            employee.Status = dto.Status;
            employee.Rate_overtime_multiplier = dto.Rate_overtime_multiplier;
            employee.Rate_latetime_multiplier = dto.Rate_latetime_multiplier;

            await _context.SaveChangesAsync();

            // Reload with related data
            await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
            await _context.Entry(employee).Reference(e => e.Shift).LoadAsync();

            return new EmployeeDto
            {
                Id = employee.Id,
                Emp_name = employee.Emp_name,
                Code = employee.Code,
                Salary = employee.Salary,
                Gender = employee.Gender,
                Age = employee.Age,
                Status = employee.Status,
                Department_id = employee.Department_id,
                Shift_id = employee.Shift_id,
                Department_name = employee.Department?.Department_name,
                Shift_name = employee.Shift?.Shift_name,
                Rate_overtime_multiplier = employee.Rate_overtime_multiplier,
                Rate_latetime_multiplier = employee.Rate_latetime_multiplier
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendences)
                .Include(e => e.Advances)
                .Include(e => e.Bounes)
                .Include(e => e.Deductions)
                .Include(e => e.PayRolls)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return false;

            // Check if employee has related records
            if (employee.Attendences.Any() || employee.PayRolls.Any())
            {
                throw new InvalidOperationException("Cannot delete employee with attendance or payroll records. Consider changing status to 'Inactive' instead.");
            }

            // Delete related financial records if any
            _context.Advances.RemoveRange(employee.Advances);
            _context.Bounes.RemoveRange(employee.Bounes);
            _context.Deductions.RemoveRange(employee.Deductions);

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            var query = _context.Employees.Where(e => e.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<EmployeeDto>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Where(e => e.Emp_name.ToLower().Contains(lowerSearchTerm) ||
                           e.Code.ToLower().Contains(lowerSearchTerm))
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Emp_name = e.Emp_name,
                    Code = e.Code,
                    Salary = e.Salary,
                    Gender = e.Gender,
                    Age = e.Age,
                    Status = e.Status,
                    Department_id = e.Department_id,
                    Shift_id = e.Shift_id,
                    Department_name = e.Department != null ? e.Department.Department_name : null,
                    Shift_name = e.Shift != null ? e.Shift.Shift_name : null,
                    Rate_overtime_multiplier = e.Rate_overtime_multiplier,
                    Rate_latetime_multiplier = e.Rate_latetime_multiplier
                })
                .ToListAsync();
        }
    }
}
