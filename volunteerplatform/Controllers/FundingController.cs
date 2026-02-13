using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Controllers
{
    public class FundingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FundingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Donate(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Organizer)
                .FirstOrDefaultAsync(i => i.Id == id);

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
            var initiative = await _context.Initiatives.FindAsync(donation.InitiativeId);
            if (initiative == null) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    donation.DonorId = user.Id;
                }

                donation.DonatedOn = DateTime.Now;

                // Simulate processing
                _context.Donations.Add(donation);
                
                initiative.CurrentAmount += donation.Amount;
                
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Success), new { id = donation.Id });
            }

            ViewBag.Initiative = initiative;
            return View("Donate", donation);
        }

        public async Task<IActionResult> Success(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Initiative)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donation == null) return NotFound();

            return View(donation);
        }
    }
}
