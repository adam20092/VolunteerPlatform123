using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public enum MissionStatus
    {
        Active,
        Filled,
        Finished
    }

    public class Initiative
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime DateAndTime { get; set; }

        public int RequiredVolunteers { get; set; }
        
        public string? RequiredSkills { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.Active;

        // Foreign Key to Organizer
        public string? OrganizerId { get; set; }
        [ForeignKey("OrganizerId")]
        public ApplicationUser? Organizer { get; set; }

        public ICollection<Enrolment>? Enrolments { get; set; }
    }
}
