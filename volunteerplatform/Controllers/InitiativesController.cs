using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    public class InitiativesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InitiativesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Initiatives
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var initiatives = from i in _context.Initiatives.Include(i => i.Organizer)
                             select i;

            if (!string.IsNullOrEmpty(searchString))
            {
                initiatives = initiatives.Where(s => s.Title.Contains(searchString) 
                                               || s.Location.Contains(searchString)
                                               || s.Description.Contains(searchString));
            }

            return View(await initiatives.ToListAsync());
        }

        // GET: Initiatives/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.Organizer)
                .Include(i => i.Enrolments).ThenInclude(e => e.Volunteer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

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
                initiative.OrganizerId = user.Id;
                initiative.Status = MissionStatus.Active;

                _context.Add(initiative);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(initiative);
        }

        // GET: Initiatives/MyInitiatives
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> MyInitiatives()
        {
            var user = await _userManager.GetUserAsync(User);
            var myInitiatives = await _context.Initiatives
                .Where(i => i.OrganizerId == user.Id)
                .ToListAsync();
            return View("Index", myInitiatives);
        }

        // GET: Initiatives/Delete/5
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var initiative = await _context.Initiatives
                .Include(i => i.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
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
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (initiative.OrganizerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Initiatives.Remove(initiative);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
