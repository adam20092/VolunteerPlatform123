using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Team Leader (Organizer or experienced Volunteer)
        public string? LeaderId { get; set; }
        [ForeignKey("LeaderId")]
        public ApplicationUser? Leader { get; set; }

        // Members of the team
        public ICollection<TeamMember>? Members { get; set; }
        
        // Initiatives this team is assigned to
        public ICollection<Initiative>? Initiatives { get; set; }
    }

    public class TeamMember
    {
        public int Id { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; } = null!;

        public string MemberId { get; set; } = string.Empty;
        [ForeignKey("MemberId")]
        public ApplicationUser Member { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.Now;
        
        public string? Role { get; set; } // e.g., "Coordinator", "Technician", "General Volunteer"
    }
}
