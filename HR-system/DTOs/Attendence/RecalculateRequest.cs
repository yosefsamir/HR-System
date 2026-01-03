namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// Request DTO for attendance recalculation
    /// </summary>
    public class RecalculateRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Optional: Filter by specific employee ID
        /// </summary>
        public int? EmployeeId { get; set; }
        
        /// <summary>
        /// Optional: Filter by department ID
        /// </summary>
        public int? DepartmentId { get; set; }
    }
}