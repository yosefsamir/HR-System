namespace HR_system.Models.Enums
{
    /// <summary>
    /// Defines how salary is calculated for employees in a shift
    /// </summary>
    public enum SalaryCalculationType
    {
        /// <summary>
        /// Salary = (Worked Hours × Salary Per Hour) - Late Time Amount + Overtime Amount
        /// Based on actual hours worked
        /// </summary>
        Hourly = 0,

        /// <summary>
        /// Salary = (Days Attended × Daily Salary) - Late Time Amount + Overtime Amount
        /// If employee attends, they get the full daily salary regardless of exact hours
        /// </summary>
        Daily = 1
    }
}
