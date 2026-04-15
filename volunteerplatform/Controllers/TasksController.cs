using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Tasks/Create
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int initiativeId, string title, string description)
        {
            var initiative = await _context.Initiatives.FindAsync(initiativeId);
            if (initiative == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (initiative.OrganizerId != user?.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var task = new MissionTask
            {
                InitiativeId = initiativeId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now,
                IsCompleted = false
            };

            _context.MissionTasks.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Initiatives", new { id = initiativeId });
        }

        // POST: Tasks/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var task = await _context.MissionTasks
                .Include(t => t.Initiative)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Check if user is approved volunteer or organizer
            bool isOrganizer = task.Initiative.OrganizerId == user.Id || User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
            bool isApprovedVolunteer = await _context.Enrolments
                .AnyAsync(e => e.InitiativeId == task.InitiativeId && e.VolunteerId == user.Id && e.Status == EnrolmentStatus.Approved);

            if (!isOrganizer && !isApprovedVolunteer)
            {
                return Forbid();
            }

            task.IsCompleted = !task.IsCompleted;
            if (task.IsCompleted)
            {
                task.CompletedAt = DateTime.Now;
                task.CompletedByUserId = user.Id;
            }
            else
            {
                task.CompletedAt = null;
                task.CompletedByUserId = null;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, isCompleted = task.IsCompleted });
        }

        // POST: Tasks/Delete/5
        [HttpPost]
        [Authorize(Roles = "Organizer,Admin,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.MissionTasks
                .Include(t => t.Initiative)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (task.Initiative.OrganizerId != user?.Id && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            int initiativeId = task.InitiativeId;
            _context.MissionTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Initiatives", new { id = initiativeId });
        }
    }
}
