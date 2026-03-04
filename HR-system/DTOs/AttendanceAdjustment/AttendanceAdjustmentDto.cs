namespace HR_system.DTOs.AttendanceAdjustment
{
    public class AttendanceAdjustmentDto
    {
        public int Id { get; set; }
        public int Employee_id { get; set; }
        public string Employee_name { get; set; } = string.Empty;
        public string Employee_code { get; set; } = string.Empty;
        public string? Department_name { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string AdjustmentType { get; set; } = "Days";
        public decimal Value { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }

        public string AdjustmentType_Display => AdjustmentType == "Days" ? "أيام" : "ساعات";
        public string Value_Display => Value > 0 ? $"+{Value}" : Value.ToString();
    }
}
