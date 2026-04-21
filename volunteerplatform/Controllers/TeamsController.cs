using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamsController(ITeamService teamService, UserManager<ApplicationUser> userManager)
        {
            _teamService = teamService;
            _userManager = userManager;
        }

        // GET: Teams
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null) return NotFound();
            return View(team);
        }

        // GET: Teams/Create
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        public async Task<IActionResult> Create([Bind("Name,Description")] Team team)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null) return Challenge();
                
                await _teamService.CreateTeamAsync(team, userId);
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // POST: Teams/Join/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var result = await _teamService.JoinTeamAsync(id, userId);
            if (result)
            {
                TempData["Success"] = "Successfully joined the team!";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Teams/Leave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var result = await _teamService.LeaveTeamAsync(id, userId);
            if (result)
            {
                TempData["Info"] = "You have left the team.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
