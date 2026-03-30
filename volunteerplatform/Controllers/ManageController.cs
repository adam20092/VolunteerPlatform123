using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly IUserProfileService _userProfileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAchievementService _achievementService;

        public ManageController(
            IUserProfileService userProfileService,
            UserManager<ApplicationUser> userManager,
            IAchievementService achievementService)
        {
            _userProfileService = userProfileService;
            _userManager = userManager;
            _achievementService = achievementService;
        }

        [TempData]
        public string StatusMessage { get; set; } = null!;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var model = await _userProfileService.GetProfileAsync(userId);
            if (model == null) return NotFound($"Unable to load user with ID '{userId}'.");

            model.StatusMessage = StatusMessage;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var result = await _userProfileService.UpdateProfileAsync(userId, model);
            if (!result.Succeeded)
            {
                StatusMessage = "Error: Unexpected error when trying to update profile.";
                return RedirectToAction(nameof(Index));
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel { StatusMessage = StatusMessage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var result = await _userProfileService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = "Your password has been changed.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> Achievements()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var model = await _achievementService.GetAchievementsAsync(userId);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPersonalData()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var data = await _userProfileService.GetPersonalDataAsync(userId);
            return File(data, "application/json", "PersonalData.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var success = await _userProfileService.DeleteAccountAsync(userId);
            if (!success)
            {
                StatusMessage = "Error: Could not delete your account.";
                return RedirectToAction(nameof(Index));
            }

            return Redirect("~/");
        }
    }
}
