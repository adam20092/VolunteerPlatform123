using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    public class InitiativesController : Controller
    {
        private readonly IInitiativeService _initiativeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InitiativesController(IInitiativeService initiativeService, UserManager<ApplicationUser> userManager)
        {
            _initiativeService = initiativeService;
            _userManager = userManager;
        }

        // GET: Initiatives
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var initiatives = await _initiativeService.GetAllInitiativesAsync(searchString);
            return View(initiatives);
        }

        // GET: Initiatives/Map
        public async Task<IActionResult> Map()
        {
            var initiatives = await _initiativeService.GetActiveInitiativesWithLocationAsync();
            return View(initiatives);
        }

        // GET: Initiatives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var initiative = await _initiativeService.GetInitiativeByIdAsync(id.Value);
            if (initiative == null) return NotFound();

            return View(initiative);
        }

        // GET: Initiatives/Create
        [Authorize(Roles = "Organizer,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Initiatives/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Location,Latitude,Longitude,DateAndTime,RequiredVolunteers,RequiredSkills")] Initiative initiative)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                await _initiativeService.CreateInitiativeAsync(initiative, user.Id);
                return RedirectToAction(nameof(Index));
            }
            return View(initiative);
        }

        // GET: Initiatives/MyInitiatives
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> MyInitiatives()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var myInitiatives = await _initiativeService.GetInitiativesByOrganizerAsync(user.Id);
            return View("Index", myInitiatives);
        }

        // GET: Initiatives/Delete/5
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var initiative = await _initiativeService.GetInitiativeByIdAsync(id.Value);
            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (initiative.OrganizerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(initiative);
        }

        // POST: Initiatives/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _initiativeService.DeleteInitiativeAsync(id, user.Id, User.IsInRole("Admin"));
            if (!result) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // POST: Initiatives/Finish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Finish(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _initiativeService.FinishInitiativeAsync(id, user.Id, User.IsInRole("Admin"));
            if (!result) return NotFound();

            TempData["Success"] = "Mission marked as finished! Volunteers can now download their certificates.";
            return RedirectToAction("Manage", "Enrolments", new { id = id });
        }
    }
}
