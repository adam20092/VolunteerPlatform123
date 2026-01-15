using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public Initiative? Initiative { get; set; }

        public string? VolunteerId { get; set; }
        [ForeignKey("VolunteerId")]
        public ApplicationUser? Volunteer { get; set; }

        public string? OrganizerId { get; set; }
        [ForeignKey("OrganizerId")]
        public ApplicationUser? Organizer { get; set; }

        [Range(1, 5)]
        public int Score { get; set; }

        public string? Comment { get; set; }
    }
}
