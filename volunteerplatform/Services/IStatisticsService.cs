using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public interface IStatisticsService
    {
        Task<HomeStatsViewModel> GetHomeStatsAsync();
        Task<List<VolunteerStats>> GetLeaderboardAsync(int count = 10);
        Task<List<Initiative>> GetRecommendedInitiativesAsync(string userId, int count = 3);
    }

    public class HomeStatsViewModel
    {
        public int TotalInitiatives { get; set; }
        public int TotalVolunteers { get; set; }
        public int CompletedProjects { get; set; }
        public int TotalDonationsCount { get; set; }
    }
}
