using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly ApplicationDbContext _context;

        public AchievementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AchievementsViewModel> GetAchievementsAsync(string userId)
        {
            // ── Fetch raw data ────────────────────────────────────────────────
            var completedEnrolments = await _context.Enrolments
                .Include(e => e.Initiative)
                .Where(e => e.VolunteerId == userId && e.Status == EnrolmentStatus.Approved)
                .ToListAsync();

            int completedMissions = completedEnrolments.Count;

            var donationCount = await _context.Donations
                .Where(d => d.DonorId == userId)
                .CountAsync();

            var totalDonated = await _context.Donations
                .Where(d => d.DonorId == userId)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var user = await _context.Users.FindAsync(userId);
            int rating = user?.Rating ?? 0;

            // Distinct locations of completed missions
            var distinctLocations = completedEnrolments
                .Select(e => e.Initiative?.Location)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            // ── Points calculation (mirrors StatisticsService) ────────────────
            int totalPoints = (completedMissions * 100) + (rating * 10) + (donationCount * 50);

            // ── Define all achievements ───────────────────────────────────────
            var achievements = new List<AchievementItem>
            {
                // —— Missions ————————————————————————————————————————
                new AchievementItem
                {
                    Key         = "first_steps",
                    Title       = "First Steps",
                    Description = "Complete your very first volunteering mission.",
                    Icon        = "bi-flag-fill",
                    Color       = "#4CAF50",
                    Category    = "Missions",
                    Progress    = Math.Min(completedMissions, 1),
                    MaxProgress = 1,
                    Unlocked    = completedMissions >= 1
                },
                new AchievementItem
                {
                    Key         = "helping_hand",
                    Title       = "Helping Hand",
                    Description = "Complete 5 volunteering missions.",
                    Icon        = "bi-hand-thumbs-up-fill",
                    Color       = "#2196F3",
                    Category    = "Missions",
                    Progress    = Math.Min(completedMissions, 5),
                    MaxProgress = 5,
                    Unlocked    = completedMissions >= 5
                },
                new AchievementItem
                {
                    Key         = "rising_star",
                    Title       = "Rising Star",
                    Description = "Complete 10 volunteering missions and shine bright.",
                    Icon        = "bi-star-fill",
                    Color       = "#FF9800",
                    Category    = "Missions",
                    Progress    = Math.Min(completedMissions, 10),
                    MaxProgress = 10,
                    Unlocked    = completedMissions >= 10
                },
                new AchievementItem
                {
                    Key         = "veteran",
                    Title       = "Veteran",
                    Description = "Complete 25 missions — a true pillar of the community.",
                    Icon        = "bi-shield-fill-check",
                    Color       = "#9C27B0",
                    Category    = "Missions",
                    Progress    = Math.Min(completedMissions, 25),
                    MaxProgress = 25,
                    Unlocked    = completedMissions >= 25
                },
                new AchievementItem
                {
                    Key         = "legend",
                    Title       = "Legend",
                    Description = "Complete 50 missions and become a living legend.",
                    Icon        = "bi-trophy-fill",
                    Color       = "#B71C1C",
                    Category    = "Missions",
                    Progress    = Math.Min(completedMissions, 50),
                    MaxProgress = 50,
                    Unlocked    = completedMissions >= 50
                },

                // —— Donations ————————————————————————————————————————
                new AchievementItem
                {
                    Key         = "generous_heart",
                    Title       = "Generous Heart",
                    Description = "Make your very first donation to a cause.",
                    Icon        = "bi-heart-fill",
                    Color       = "#E91E63",
                    Category    = "Donations",
                    Progress    = Math.Min(donationCount, 1),
                    MaxProgress = 1,
                    Unlocked    = donationCount >= 1
                },
                new AchievementItem
                {
                    Key         = "philanthropist",
                    Title       = "Philanthropist",
                    Description = "Support 5 different initiatives with donations.",
                    Icon        = "bi-cash-coin",
                    Color       = "#009688",
                    Category    = "Donations",
                    Progress    = Math.Min(donationCount, 5),
                    MaxProgress = 5,
                    Unlocked    = donationCount >= 5
                },
                new AchievementItem
                {
                    Key         = "diamond_donor",
                    Title       = "Diamond Donor",
                    Description = "Donate a total of 1,000 or more across all initiatives.",
                    Icon        = "bi-gem",
                    Color       = "#00BCD4",
                    Category    = "Donations",
                    Progress    = (int)Math.Min(totalDonated, 1000),
                    MaxProgress = 1000,
                    Unlocked    = totalDonated >= 1000
                },

                // —— Reputation ———————————————————————————————————————
                new AchievementItem
                {
                    Key         = "top_rated",
                    Title       = "Top Rated",
                    Description = "Earn a rating of 5 or higher from organizers.",
                    Icon        = "bi-patch-check-fill",
                    Color       = "#FFC107",
                    Category    = "Reputation",
                    Progress    = Math.Min(rating, 5),
                    MaxProgress = 5,
                    Unlocked    = rating >= 5
                },
                new AchievementItem
                {
                    Key         = "superstar",
                    Title       = "Superstar",
                    Description = "Achieve a rating of 10 — you are exceptional.",
                    Icon        = "bi-award-fill",
                    Color       = "#FF5722",
                    Category    = "Reputation",
                    Progress    = Math.Min(rating, 10),
                    MaxProgress = 10,
                    Unlocked    = rating >= 10
                },

                // —— Explorer ————————————————————————————————————————
                new AchievementItem
                {
                    Key         = "explorer",
                    Title       = "Explorer",
                    Description = "Volunteer in 3 different cities or locations.",
                    Icon        = "bi-compass-fill",
                    Color       = "#3F51B5",
                    Category    = "Explorer",
                    Progress    = Math.Min(distinctLocations, 3),
                    MaxProgress = 3,
                    Unlocked    = distinctLocations >= 3
                },
                new AchievementItem
                {
                    Key         = "globe_trotter",
                    Title       = "Globe Trotter",
                    Description = "Volunteer in 7 different locations across the country.",
                    Icon        = "bi-globe2",
                    Color       = "#607D8B",
                    Category    = "Explorer",
                    Progress    = Math.Min(distinctLocations, 7),
                    MaxProgress = 7,
                    Unlocked    = distinctLocations >= 7
                },
            };

            // ── Determine overall rank badge ──────────────────────────────────
            string overallBadge, overallColor;
            if (totalPoints >= 1000)      { overallBadge = "Legend";      overallColor = "#B71C1C"; }
            else if (totalPoints >= 500)  { overallBadge = "Hero";        overallColor = "#9C27B0"; }
            else if (totalPoints >= 200)  { overallBadge = "Rising Star"; overallColor = "#FF9800"; }
            else if (totalPoints >= 50)   { overallBadge = "Newcomer";    overallColor = "#2196F3"; }
            else                          { overallBadge = "Newbie";      overallColor = "#607D8B"; }

            return new AchievementsViewModel
            {
                Achievements       = achievements,
                TotalPoints        = totalPoints,
                CompletedMissions  = completedMissions,
                DonationCount      = donationCount,
                OverallBadge       = overallBadge,
                OverallBadgeColor  = overallColor
            };
        }
    }
}
