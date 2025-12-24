using HR_system.Data;
using HR_system.DTOs.Department;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Department_name = d.Department_name,
                    EmployeeCount = d.Employees.Count
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return null;

            return new DepartmentDto
            {
                Id = department.Id,
                Department_name = department.Department_name,
                EmployeeCount = department.Employees.Count
            };
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Department_name = dto.Department_name
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.Id,
                Department_name = department.Department_name,
                EmployeeCount = 0
            };
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return null;

            department.Department_name = dto.Department_name;

            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.Id,
                Department_name = department.Department_name,
                EmployeeCount = await _context.Employees.CountAsync(e => e.Department_id == id)
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return false;

            // Check if department has employees
            var hasEmployees = await _context.Employees.AnyAsync(e => e.Department_id == id);
            if (hasEmployees)
            {
                throw new InvalidOperationException("Cannot delete department with assigned employees.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Departments.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _context.Departments.Where(d => d.Department_name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
