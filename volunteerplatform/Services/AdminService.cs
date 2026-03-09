using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();
        Task<List<UserAdminViewModel>> GetAllUsersAsync();
        Task<List<Enrolment>> GetAllRequestsAsync();
    }

    public class AdminService : IAdminService
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            return new AdminDashboardViewModel
            {
                TotalVolunteers = (await _userManager.GetUsersInRoleAsync("Volunteer")).Count,
                TotalOrganizers = (await _userManager.GetUsersInRoleAsync("Organizer")).Count,
                TotalMission = await _context.Initiatives.CountAsync(),
                CompletedMissions = await _context.Initiatives.CountAsync(i => i.Status == MissionStatus.Finished),
                PendingRequests = await _context.Enrolments.CountAsync(e => e.Status == EnrolmentStatus.Pending),
                RecentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync()
            };
        }

        public async Task<List<UserAdminViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var result = new List<UserAdminViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserAdminViewModel { User = u, Roles = roles });
            }

            // Sort: Admin first, then Organizer, then Volunteer
            return result
                .OrderBy(x => x.RoleDisplay == "Admin" ? 0 : x.RoleDisplay == "Organizer" ? 1 : 2)
                .ThenBy(x => x.User.FullName)
                .ToList();
        }

        public async Task<List<Enrolment>> GetAllRequestsAsync()
        {
            return await _context.Enrolments
                .Include(e => e.Initiative)
                .Include(e => e.Volunteer)
                .OrderByDescending(e => e.AppliedOn)
                .ToListAsync();
        }
    }
}
