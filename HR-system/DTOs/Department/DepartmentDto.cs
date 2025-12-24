namespace HR_system.DTOs.Department
{
    /// <summary>
    /// Used for displaying department data
    /// </summary>
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Department_name { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }
}
