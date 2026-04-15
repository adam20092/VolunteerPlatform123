using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IReportService _reportService;

        public AdminController(IAdminService adminService, IReportService reportService)
        {
            _adminService = adminService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await _adminService.GetDashboardStatsAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Requests()
        {
            var requests = await _adminService.GetAllRequestsAsync();
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUser = await _adminService.GetAllUsersAsync();
            var targetUserViewModel = currentUser.FirstOrDefault(u => u.User.Id == userId);

            if (targetUserViewModel == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            // Safety Checks:
            // 1. Cannot delete self
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == userId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            // Restriction removed to allow admin to manage all users

            var success = await _adminService.DeleteUserAsync(userId);
            if (success)
            {
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "An error occurred while deleting the user.";
            }

            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> ExportInitiatives()
        {
            var data = await _reportService.GenerateInitiativesCsvAsync();
            return File(data, "text/csv", $"Initiatives_Report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}