using Microsoft.AspNetCore.Identity;

namespace HR_system.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Extended properties
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}