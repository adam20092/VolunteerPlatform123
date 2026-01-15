using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class EnrolmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrolmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Enrolments/Apply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> Apply(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Check if already enrolled
            var exists = await _context.Enrolments
                .AnyAsync(e => e.InitiativeId == id && e.VolunteerId == user.Id);

            if (exists)
            {
                TempData["Message"] = "You have already applied for this mission.";
                return RedirectToAction("Details", "Initiatives", new { id = id });
            }

            var enrolment = new Enrolment
            {
                InitiativeId = id,
                VolunteerId = user.Id,
                Status = EnrolmentStatus.Pending,
                AppliedOn = DateTime.Now
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application submitted successfully!";
            return RedirectToAction("Details", "Initiatives", new { id = id });
        }

        // GET: Enrolments/MyApplications
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            var applications = await _context.Enrolments
                .Include(e => e.Initiative)
                .Where(e => e.VolunteerId == user.Id)
                .OrderByDescending(e => e.AppliedOn)
                .ToListAsync();

            return View(applications);
        }

        // GET: Enrolments/Manage/5 (For Organizers)
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Manage(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Enrolments).ThenInclude(e => e.Volunteer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Security check: Only owner or admin can manage
            var user = await _userManager.GetUserAsync(User);
            if (initiative.OrganizerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(initiative);
        }

        // POST: Enrolments/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, EnrolmentStatus status)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();

            enrolment.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage", new { id = enrolment.InitiativeId });
        }
    }
}
