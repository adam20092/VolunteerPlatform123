using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalVolunteers = await _userManager.GetUsersInRoleAsync("Volunteer").ContinueWith(t => t.Result.Count),
                TotalOrganizers = await _userManager.GetUsersInRoleAsync("Organizer").ContinueWith(t => t.Result.Count),
                TotalMission = await _context.Initiatives.CountAsync(),
                CompletedMissions = await _context.Initiatives.CountAsync(i => i.Status == MissionStatus.Finished),
                PendingRequests = await _context.Enrolments.CountAsync(e => e.Status == EnrolmentStatus.Pending),
                RecentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Requests()
        {
            var requests = await _context.Enrolments
                .Include(e => e.Initiative)
                .Include(e => e.Volunteer)
                .OrderByDescending(e => e.AppliedOn)
                .ToListAsync();

            return View(requests);
        }
    }

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