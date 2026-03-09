using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}