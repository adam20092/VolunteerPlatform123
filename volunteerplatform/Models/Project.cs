using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? OrganizerId { get; set; }
        [ForeignKey("OrganizerId")]
        public ApplicationUser? Organizer { get; set; }

        // A Project can have multiple Initiatives (Missions)
        public ICollection<Initiative>? Initiatives { get; set; }
    }
}
