using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<HomeStatsViewModel> GetHomeStatsAsync()
        {
            return new HomeStatsViewModel
            {
                TotalInitiatives = await _context.Initiatives.CountAsync(),
                TotalVolunteers = (await _userManager.GetUsersInRoleAsync("Volunteer")).Count,
                CompletedProjects = await _context.Initiatives.CountAsync(i => i.Status == MissionStatus.Finished),
                TotalDonationsCount = await _context.Donations.CountAsync()
            };
        }

        public async Task<List<VolunteerStats>> GetLeaderboardAsync(int count = 10)
        {
            var volunteers = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Volunteer")))
                .Select(u => new VolunteerStats
                {
                    FullName = u.FullName ?? u.UserName,
                    CompletedMissions = _context.Enrolments.Count(e => e.VolunteerId == u.Id && e.Status == EnrolmentStatus.Approved),
                    TotalPoints = (_context.Enrolments.Count(e => e.VolunteerId == u.Id && e.Status == EnrolmentStatus.Approved) * 100) + (u.Rating * 10)
                })
                .OrderByDescending(v => v.TotalPoints)
                .Take(count)
                .ToListAsync();

            int rank = 1;
            foreach (var v in volunteers)
            {
                v.Rank = rank++;
                if (v.TotalPoints >= 1000) { v.Badge = "Legend"; v.BadgeColor = "bg-warning text-dark"; }
                else if (v.TotalPoints >= 500) { v.Badge = "Hero"; v.BadgeColor = "bg-primary"; }
                else if (v.TotalPoints >= 100) { v.Badge = "Rising Star"; v.BadgeColor = "bg-success"; }
                else { v.Badge = "Newbie"; v.BadgeColor = "bg-secondary"; }
            }

            return volunteers;
        }

        public async Task<List<Initiative>> GetRecommendedInitiativesAsync(string userId, int count = 3)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Skills))
            {
                return new List<Initiative>();
            }

            var userSkills = user.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            var initiatives = await _context.Initiatives
                .Include(i => i.Organizer)
                .Where(i => i.Status == MissionStatus.Active)
                .ToListAsync();

            return initiatives
                .Where(i => !string.IsNullOrEmpty(i.RequiredSkills) && 
                            userSkills.Any(s => i.RequiredSkills.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Take(count)
                .ToList();
        }
    }
}
