using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class EnrolmentsController : Controller
    {
        private readonly IEnrolmentService _enrolmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrolmentsController(IEnrolmentService enrolmentService, UserManager<ApplicationUser> userManager)
        {
            _enrolmentService = enrolmentService;
            _userManager = userManager;
        }

        // POST: Enrolments/Apply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> Apply(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            var success = await _enrolmentService.ApplyAsync(id, user.Id);

            if (!success)
            {
                TempData["Message"] = "You have already applied for this mission.";
            }
            else
            {
                TempData["Success"] = "Application submitted successfully!";
            }

            return RedirectToAction("Details", "Initiatives", new { id = id });
        }

        // GET: Enrolments/MyApplications
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var applications = await _enrolmentService.GetEnrolmentsByVolunteerAsync(user.Id);
            return View(applications);
        }

        // GET: Enrolments/Manage/5 (For Organizers)
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Manage(int id)
        {
            var initiative = await _enrolmentService.GetInitiativeWithEnrolmentsAsync(id);
            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

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
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null) return NotFound();

            await _enrolmentService.UpdateStatusAsync(id, status);

            return RedirectToAction("Manage", new { id = enrolment.InitiativeId });
        }

        // GET: Enrolments/Certificate/5
        public async Task<IActionResult> Certificate(int id)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (enrolment.VolunteerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _enrolmentService.EnsureCertificateCodeAsync(id);

            return View(enrolment);
        }
    }
}
