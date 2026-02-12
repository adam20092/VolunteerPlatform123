using System.Collections.Generic;

namespace volunteerplatform.Models.ViewModels
{
    public class LeaderboardViewModel
    {
        public List<VolunteerStats> TopVolunteers { get; set; } = new List<VolunteerStats>();
    }

    public class VolunteerStats
    {
        public string FullName { get; set; }
        public int CompletedMissions { get; set; }
        public int TotalPoints { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public int Rank { get; set; }
    }
}
