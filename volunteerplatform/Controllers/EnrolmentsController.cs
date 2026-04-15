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
        private readonly IInitiativeService _initiativeService;
        private readonly IReportService _reportService;

        public EnrolmentsController(
            IEnrolmentService enrolmentService, 
            UserManager<ApplicationUser> userManager,
            IInitiativeService initiativeService,
            IReportService reportService)
        {
            _enrolmentService = enrolmentService;
            _userManager = userManager;
            _initiativeService = initiativeService;
            _reportService = reportService;
        }

        // POST: Enrolments/Apply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> Apply(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var initiative = await _initiativeService.GetInitiativeByIdAsync(id);
            if (initiative == null) return NotFound();

            if (!string.IsNullOrEmpty(initiative.RequiredSkills))
            {
                var reqSkills = initiative.RequiredSkills.Split(',').Select(s => s.Trim()).ToList();
                var userSkills = user.Skills?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();

                if (!reqSkills.Intersect(userSkills, StringComparer.OrdinalIgnoreCase).Any())
                {
                    TempData["Error"] = "Нямате нито едно от изискваните умения за тази мисия.";
                    return RedirectToAction("Details", "Initiatives", new { id = id });
                }
            }
            
            var success = await _enrolmentService.ApplyAsync(id, user.Id);

            if (!success)
            {
                TempData["Message"] = "You applied for this mission.";
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
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        public async Task<IActionResult> Manage(int id)
        {
            var initiative = await _enrolmentService.GetInitiativeWithEnrolmentsAsync(id);
            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (initiative.OrganizerId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            return View(initiative);
        }

        // POST: Enrolments/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, EnrolmentStatus status)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null) return NotFound();

            await _enrolmentService.UpdateStatusAsync(id, status);

            return RedirectToAction("Manage", new { id = enrolment.InitiativeId });
        }

        // POST: Enrolments/Remove/5
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, string? returnUrl = null)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Check if user is Admin or the Organizer of the initiative
            if (enrolment.Initiative.OrganizerId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var initiativeId = enrolment.InitiativeId;
            await _enrolmentService.DeleteEnrolmentAsync(id);

            TempData["Success"] = "Candidate removed successfully.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Manage", new { id = initiativeId });
        }

        // GET: Enrolments/Certificate/5
        public async Task<IActionResult> Certificate(int id)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (enrolment.VolunteerId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            await _enrolmentService.EnsureCertificateCodeAsync(id);

            return View(enrolment);
        }
        [HttpGet]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id);
            if (enrolment == null || enrolment.Initiative == null || enrolment.Volunteer == null) return NotFound();

            if (enrolment.Initiative.Status != MissionStatus.Finished)
            {
                TempData["Error"] = "Certificate is only available for finished missions.";
                return RedirectToAction("MyApplications");
            }

            var code = await _enrolmentService.EnsureCertificateCodeAsync(id);
            var html = await _reportService.GenerateCertificateHtmlAsync(
                enrolment.Volunteer.FullName!,
                enrolment.Initiative.Title!,
                enrolment.Initiative.DateAndTime.ToString("MMMM dd, yyyy"),
                code
            );

            // We return HTML as a file, and the browser can print it to PDF
            // This avoids heavy PDF libraries and works perfectly for a demo
            var data = System.Text.Encoding.UTF8.GetBytes(html);
            return File(data, "text/html", $"Certificate_{enrolment.Id}.html");
        }
    }
}
