using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace volunteerplatform.Models
{
    public enum EnrolmentStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Enrolment
    {
        public int Id { get; set; }

        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public Initiative Initiative { get; set; } = null!;

        public string VolunteerId { get; set; } = string.Empty;
        [ForeignKey("VolunteerId")]
        public ApplicationUser Volunteer { get; set; } = null!;

        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Pending;
        
        public DateTime AppliedOn { get; set; } = DateTime.Now;

        public string? CertificateCode { get; set; }
    }
}
