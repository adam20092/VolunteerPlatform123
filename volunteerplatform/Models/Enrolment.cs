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
        public Initiative Initiative { get; set; }

        public string VolunteerId { get; set; }
        [ForeignKey("VolunteerId")]
        public ApplicationUser Volunteer { get; set; }

        public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Pending;
        
        public DateTime AppliedOn { get; set; } = DateTime.Now;
    }
}
