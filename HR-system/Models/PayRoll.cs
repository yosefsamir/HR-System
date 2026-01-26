using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HR_system.Models
{
    public class PayRoll
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Employee")]
        public int Employee_id { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        // Employee Info (stored to avoid re-lookup)
        [StringLength(100)]
        public string EmployeeName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? DepartmentName { get; set; }
        
        [StringLength(100)]
        public string? ShiftName { get; set; }

        // Month Configuration
        public int WorkingDaysInMonth { get; set; }
        public int HolidaysInMonth { get; set; }

        // Base Salary Info
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryPerHour { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalaryPerDay { get; set; }

        [StringLength(20)]
        public string SalaryCalculationType { get; set; } = "Hourly";

        [StringLength(20)]
        public string SalaryCalculationTypeDisplay { get; set; } = "بالساعة";

        // Working Hours Info
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShiftHoursPerDay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedWorkingHours { get; set; }

        // Attendance Summary
        public int ActualPresentDays { get; set; }
        public int AbsentDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualWorkedMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualWorkedHours { get; set; }

        // Overtime Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeMultiplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimeAmount { get; set; }

        // Late Time Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal LateTimeMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateTimeHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateTimeMultiplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateTimeDeduction { get; set; }

        // Early Departure Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal EarlyDepartureMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EarlyDepartureHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EarlyDepartureMultiplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EarlyDepartureDeduction { get; set; }

        // Net Time Difference
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTimeDifferenceHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTimeDifferenceAmount { get; set; }

        // Permission
        [Column(TypeName = "decimal(18,2)")]
        public decimal PermissionMinutes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PermissionHours { get; set; }

        // Financial Summary
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBonuses { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAdvances { get; set; }

        // Worked Hours Salary (SalaryPerHour × ActualWorkedHours)
        [Column(TypeName = "decimal(18,2)")]
        public decimal WorkedHoursSalary { get; set; }

        // Final Calculations
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductionsAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // Payment Status
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualPaidAmount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime DateSaved { get; set; }

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}
