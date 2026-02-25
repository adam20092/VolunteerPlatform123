using volunteerplatform.Models;

namespace volunteerplatform.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalVolunteers { get; set; }
        public int TotalOrganizers { get; set; }
        public int TotalMission { get; set; }
        public int CompletedMissions { get; set; }
        public int PendingRequests { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
    }
}
