# HR System - Service Layer Implementation Guide

## Architecture Overview

This guide follows a **layered architecture** pattern:

```
┌─────────────────────────────────────────────────┐
│                  Controllers                     │
│         (HTTP Requests/Responses)               │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│                   Services                       │
│         (Business Logic Layer)                  │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│                 Repositories                     │
│         (Data Access Layer)                     │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│                  DbContext                       │
│         (Entity Framework Core)                 │
└─────────────────────────────────────────────────┘
```

---

## Step 1: Install Required NuGet Packages

Run these commands in the terminal (from the HR-system project folder):

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## Step 2: Create the DbContext

**File:** `Data/ApplicationDbContext.cs`

The DbContext is the bridge between your models and the database.

```csharp
using HR_system.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Department> Departments { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Advance> Advances { get; set; }
        public DbSet<Bounes> Bounes { get; set; }
        public DbSet<Deduction> Deductions { get; set; }
        public DbSet<Attendence> Attendences { get; set; }
        public DbSet<OverTime> OverTimes { get; set; }
        public DbSet<LateTime> LateTimes { get; set; }
        public DbSet<PayRoll> PayRolls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here if needed
        }
    }
}
```

---

## Step 3: Create DTOs (Data Transfer Objects)

**Folder:** `DTOs/`

DTOs separate your API contracts from your database models.

### File: `DTOs/Department/DepartmentDto.cs`

```csharp
namespace HR_system.DTOs.Department
{
    // Used for displaying department data
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Department_name { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }
}
```

### File: `DTOs/Department/CreateDepartmentDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Department
{
    // Used when creating a new department
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Department_name { get; set; } = string.Empty;
    }
}
```

### File: `DTOs/Department/UpdateDepartmentDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Department
{
    // Used when updating a department
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Department_name { get; set; } = string.Empty;
    }
}
```

---

## Step 4: Create Service Interface

**Folder:** `Services/Interfaces/`

Interfaces define the contract for your services.

### File: `Services/Interfaces/IDepartmentService.cs`

```csharp
using HR_system.DTOs.Department;

namespace HR_system.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
```

---

## Step 5: Create Service Implementation

**Folder:** `Services/`

### File: `Services/DepartmentService.cs`

```csharp
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
```

---

## Step 6: Create the Controller

**Folder:** `Controllers/`

### File: `Controllers/DepartmentsController.cs`

```csharp
using HR_system.DTOs.Department;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetAllAsync();
            return View(departments);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Check for unique name
            if (!await _departmentService.IsNameUniqueAsync(dto.Department_name))
            {
                ModelState.AddModelError("Department_name", "A department with this name already exists.");
                return View(dto);
            }

            await _departmentService.CreateAsync(dto);
            TempData["Success"] = "Department created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            var dto = new UpdateDepartmentDto
            {
                Department_name = department.Department_name
            };

            ViewBag.DepartmentId = id;
            return View(dto);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentId = id;
                return View(dto);
            }

            // Check for unique name (excluding current department)
            if (!await _departmentService.IsNameUniqueAsync(dto.Department_name, id))
            {
                ModelState.AddModelError("Department_name", "A department with this name already exists.");
                ViewBag.DepartmentId = id;
                return View(dto);
            }

            var result = await _departmentService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound();
            }

            TempData["Success"] = "Department updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _departmentService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                TempData["Success"] = "Department deleted successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
```

---

## Step 7: Register Services in Program.cs

**File:** `Program.cs`

```csharp
using HR_system.Data;
using HR_system.Services;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services (Dependency Injection)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ... rest of configuration
```

---

## Step 8: Configure Connection String

**File:** `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=HRSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## Step 9: Create Database Migration

Run these commands in terminal:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

---

## Folder Structure After Implementation

```
HR-system/
├── Controllers/
│   ├── DepartmentsController.cs
│   └── HomeController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/
│   └── Department/
│       ├── CreateDepartmentDto.cs
│       ├── DepartmentDto.cs
│       └── UpdateDepartmentDto.cs
├── Models/
│   ├── Department.cs
│   ├── Employee.cs
│   └── ... (other models)
├── Services/
│   ├── Interfaces/
│   │   └── IDepartmentService.cs
│   └── DepartmentService.cs
├── Views/
│   └── Departments/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       ├── Details.cshtml
│       └── Delete.cshtml
└── Program.cs
```

---

## Benefits of This Structure

| Layer | Responsibility | Benefit |
|-------|----------------|---------|
| **DTOs** | Data transfer | Separates API from database models |
| **Services** | Business logic | Reusable, testable logic |
| **Controllers** | HTTP handling | Thin controllers, focused on routing |
| **DbContext** | Data access | Centralized database configuration |

---

## Next Steps

1. Create the actual files following this guide
2. Create Views for the Department CRUD operations
3. Apply the same pattern for other models (Employee, Shift, etc.)
4. Add validation and error handling as needed

---

## Tips

- **Keep controllers thin**: Move business logic to services
- **Use async/await**: For better performance
- **Validate input**: Both in DTOs and services
- **Handle exceptions**: Gracefully handle errors
- **Use dependency injection**: Makes testing easier

