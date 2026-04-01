using Microsoft.AspNetCore.Identity;

namespace HR_system.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description {get ; set ;}
        public bool IsActive {get; set; }

        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; } 
    }
}