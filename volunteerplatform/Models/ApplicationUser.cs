using Microsoft.AspNetCore.Identity;

namespace volunteerplatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        
        // Volunteer Specifics
        public int? Age { get; set; }
        public string? Skills { get; set; } // Comma separated for now
        public string? Availability { get; set; }
        public string? Location { get; set; }
        public int Rating { get; set; } = 0;

        // Organizer Specifics
        public string? OrganizationName { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}
