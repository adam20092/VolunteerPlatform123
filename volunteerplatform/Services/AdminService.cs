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
        Task<bool> DeleteUserAsync(string userId);
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

            // Sort: SuperAdmin first, then Admin, then Organizer, then Volunteer
            return result
                .OrderBy(x => x.RoleDisplay == "SuperAdmin" ? 0 : x.RoleDisplay == "Admin" ? 1 : x.RoleDisplay == "Organizer" ? 2 : 3)
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

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // 1. Delete enrolments where user is a volunteer
            var enrolments = _context.Enrolments.Where(e => e.VolunteerId == userId);
            _context.Enrolments.RemoveRange(enrolments);

            // 2. Delete ratings involving this user
            // 2a. User as volunteer
            var volunteerRatings = _context.Ratings.Where(r => r.VolunteerId == userId);
            _context.Ratings.RemoveRange(volunteerRatings);

            // 2b. User as organizer (ratings they gave)
            var organizerRatings = _context.Ratings.Where(r => r.OrganizerId == userId);
            _context.Ratings.RemoveRange(organizerRatings);

            // 3. Anonymize donations (or delete them)
            var donations = _context.Donations.Where(d => d.DonorId == userId);
            foreach (var donation in donations)
            {
                donation.DonorId = null; // Keep the record but remove the linked user
            }

            // 4. Handle Initiatives organized by this user
            var initiatives = await _context.Initiatives
                .Include(i => i.Enrolments)
                .Include(i => i.Donations)
                .Where(i => i.OrganizerId == userId)
                .ToListAsync();

            foreach (var initiative in initiatives)
            {
                // Explicitly remove related data if needed, or let DB cascade handle it
                if (initiative.Enrolments != null) _context.Enrolments.RemoveRange(initiative.Enrolments);
                if (initiative.Donations != null) _context.Donations.RemoveRange(initiative.Donations);
                
                _context.Initiatives.Remove(initiative);
            }

            await _context.SaveChangesAsync();

            // 5. Delete the user
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
