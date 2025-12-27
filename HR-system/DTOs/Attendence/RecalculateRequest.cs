namespace HR_system.DTOs.Attendence
{
    /// <summary>
    /// Request DTO for attendance recalculation
    /// </summary>
    public class RecalculateRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}