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
        }
    }
}
