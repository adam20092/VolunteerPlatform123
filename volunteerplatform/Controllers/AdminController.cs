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
                RecentUsers = await _userManager.Users.OrderByDescending(u => u.Id).Take(5).ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalVolunteers { get; set; }
        public int TotalOrganizers { get; set; }
        public int TotalMission { get; set; }
        public int CompletedMissions { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; }
    }
}