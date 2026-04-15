using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public class MissionTask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public string? CompletedByUserId { get; set; }
        [ForeignKey("CompletedByUserId")]
        public ApplicationUser? CompletedByUser { get; set; }

        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public Initiative Initiative { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
