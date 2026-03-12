using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public interface IAchievementService
    {
        Task<AchievementsViewModel> GetAchievementsAsync(string userId);
    }
}
