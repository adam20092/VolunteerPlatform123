using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using volunteerplatform.Models;
using volunteerplatform.Services;

namespace volunteerplatform.Controllers
{
    public class FundingController : Controller
    {
        private readonly IFundingService _fundingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FundingController(IFundingService fundingService, UserManager<ApplicationUser> userManager)
        {
            _fundingService = fundingService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Donate(int id)
        {
            var initiative = await _fundingService.GetInitiativeForDonationAsync(id);

            if (initiative == null) return NotFound();

            if (initiative.TargetAmount == null || initiative.TargetAmount <= 0)
            {
                TempData["Error"] = "This initiative is not accepting donations.";
                return RedirectToAction("Details", "Initiatives", new { id = id });
            }

            ViewBag.Initiative = initiative;
            return View(new Donation { InitiativeId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDonation(Donation donation)
        {
            if (!ModelState.IsValid)
            {
                var initiative = await _fundingService.GetInitiativeForDonationAsync(donation.InitiativeId);
                ViewBag.Initiative = initiative;
                return View("Donate", donation);
            }

            var user = await _userManager.GetUserAsync(User);
            var donationId = await _fundingService.ProcessDonationAsync(donation, user?.Id);

            if (donationId == 0) return NotFound();

            return RedirectToAction(nameof(Success), new { id = donationId });
        }

        public async Task<IActionResult> Success(int id)
        {
            var donation = await _fundingService.GetDonationDetailsAsync(id);
            if (donation == null) return NotFound();

            return View(donation);
        }
    }
}
