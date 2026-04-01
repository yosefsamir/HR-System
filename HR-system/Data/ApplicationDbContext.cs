using HR_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser , ApplicationRole , Guid> 
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
        public DbSet<EarlyDeparture> EarlyDepartures { get; set; }
        public DbSet<PayRoll> PayRolls { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<AttendanceAdjustment> AttendanceAdjustments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {       
            base.OnModelCreating(modelBuilder);

            // Rename tables
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Configure one-to-one relationship: Attendence -> OverTime
            modelBuilder.Entity<OverTime>()
                .HasOne(o => o.Attendence)
                .WithOne(a => a.OverTime)
                .HasForeignKey<OverTime>(o => o.Attendence_id);

            // Configure one-to-one relationship: Attendence -> LateTime
            modelBuilder.Entity<LateTime>()
                .HasOne(l => l.Attendence)
                .WithOne(a => a.LateTime)
                .HasForeignKey<LateTime>(l => l.Attendence_id);

            // Configure one-to-one relationship: Attendence -> EarlyDeparture
            modelBuilder.Entity<EarlyDeparture>()
                .HasOne(e => e.Attendence)
                .WithOne(a => a.EarlyDeparture)
                .HasForeignKey<EarlyDeparture>(e => e.Attendence_id);
        }
    }
}
