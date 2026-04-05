using HR_system.Data;
using HR_system.Middleware;
using HR_system.Models;
using HR_system.Repositories;
using HR_system.Services;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.FontDiscoveryPaths.Add(Path.Combine(builder.Environment.WebRootPath, "fonts"));
QuestPDF.Settings.UseEnvironmentFonts = false;

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity service
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // User settings
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie (login path, etc.)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = false;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

// Register Repositories
builder.Services.AddScoped<IAttendenceRepository, AttendenceRepository>();
builder.Services.AddScoped<IAttendanceRepository>(sp => (IAttendanceRepository)sp.GetRequiredService<IAttendenceRepository>());
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IMonthlyAttendanceRepository, MonthlyAttendanceRepository>();

// Register Services (Dependency Injection)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeExcelService, EmployeeExcelService>();
builder.Services.AddScoped<IBounesService, BounesService>();
builder.Services.AddScoped<IBonusService>(sp => (IBonusService)sp.GetRequiredService<IBounesService>());
builder.Services.AddScoped<IDeductionService, DeductionService>();
builder.Services.AddScoped<IAdvanceService, AdvanceService>();
builder.Services.AddScoped<IAttendanceAdjustmentService, AttendanceAdjustmentService>();
builder.Services.AddScoped<IAttendenceService, AttendenceService>();
builder.Services.AddScoped<IAttendanceService>(sp => (IAttendanceService)sp.GetRequiredService<IAttendenceService>());
builder.Services.AddScoped<IAttendanceExcelService, AttendanceExcelService>();
builder.Services.AddScoped<ISalaryService, SalaryService>();
builder.Services.AddScoped<IMonthlyAttendanceService, MonthlyAttendanceService>();
builder.Services.AddScoped<HR_system.Domain.SalaryCalculation.SalaryCalculator>();
builder.Services.AddScoped<IPayRollWhatsAppService, PayRollWhatsAppService>();
builder.Services.AddScoped<IWhatsAppSettingsService, WhatsAppSettingsService>();

builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddHttpClient<IWhatsAppService, OpenWaWhatsAppService>(client =>
{
    // OpenWA can need extra time while booting Chromium or restoring sessions.
    client.Timeout = TimeSpan.FromMinutes(4);
});
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Handle migration command for updates
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Console.WriteLine("Applying database migrations...");
    db.Database.Migrate();
    Console.WriteLine("Database migrations applied successfully.");
    return;
}

// Auto-apply migrations on startup (for production)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not apply migrations: {ex.Message}");
    }
}

// Seed roles / ensure the very first user is Admin (backfill for fresh installs)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(HR_system.Security.RoleNames.Admin))
    {
        var createRole = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = HR_system.Security.RoleNames.Admin,
            NormalizedName = HR_system.Security.RoleNames.Admin.ToUpperInvariant(),
            IsActive = true,
            CreatedOn = DateTime.Now
        });
        if (!createRole.Succeeded)
            Console.WriteLine($"Warning: Could not create role '{HR_system.Security.RoleNames.Admin}': {string.Join(", ", createRole.Errors.Select(e => e.Description))}");
    }

    // IMPORTANT:
    // - Creating the first user via /Account/Setup already assigns Admin immediately.
    // - This startup logic is only a safety net: if the DB already has users but no Admins,
    //   we only auto-assign Admin when there is exactly ONE user in the system.
    if (userManager.Users.Any())
    {
        var admins = await userManager.GetUsersInRoleAsync(HR_system.Security.RoleNames.Admin);
        if (!admins.Any())
        {
            var userCount = userManager.Users.Count();
            if (userCount == 1)
            {
                var onlyUser = userManager.Users.FirstOrDefault();
                if (onlyUser != null)
                {
                    var addRole = await userManager.AddToRoleAsync(onlyUser, HR_system.Security.RoleNames.Admin);
                    if (!addRole.Succeeded)
                        Console.WriteLine($"Warning: Could not assign '{HR_system.Security.RoleNames.Admin}' to the only user: {string.Join(", ", addRole.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Enforce login only when at least one user account exists.
// If the DB has no users yet, the system is freely accessible.
app.UseMiddleware<ConditionalAuthMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
