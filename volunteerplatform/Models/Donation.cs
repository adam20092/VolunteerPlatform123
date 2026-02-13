using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public class Donation
    {
        public int Id { get; set; }

        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public Initiative? Initiative { get; set; }

        public string? DonorId { get; set; }
        [ForeignKey("DonorId")]
        public ApplicationUser? Donor { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        public DateTime DonatedOn { get; set; } = DateTime.Now;

        public string? Message { get; set; }
    }
}
