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

    public class UserAdminViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
        public string RoleDisplay => Roles.FirstOrDefault() ?? "—";
    }
}
