using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Rate(int initiativeId, string volunteerId)
        {
            var model = new Rating
            {
                InitiativeId = initiativeId,
                VolunteerId = volunteerId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Rate(Rating rating)
        {
            if (rating.Score < 1 || rating.Score > 5)
            {
                 ModelState.AddModelError("", "Score must be between 1 and 5");
                 return View(rating);
            }

            var user = await _userManager.GetUserAsync(User);
            rating.OrganizerId = user.Id;

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage", "Enrolments", new { id = rating.InitiativeId });
        }
    }
}
