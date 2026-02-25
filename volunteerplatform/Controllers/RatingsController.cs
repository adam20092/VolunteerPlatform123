using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly IRatingService _ratingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(IRatingService ratingService, UserManager<ApplicationUser> userManager)
        {
            _ratingService = ratingService;
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var success = await _ratingService.SubmitRatingAsync(rating, user.Id);

            if (!success)
            {
                ModelState.AddModelError("", "Score must be between 1 and 5");
                return View(rating);
            }

            return RedirectToAction("Manage", "Enrolments", new { id = rating.InitiativeId });
        }
    }
}
