using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public class FundingService : IFundingService
    {
        private readonly ApplicationDbContext _context;

        public FundingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Initiative?> GetInitiativeForDonationAsync(int id)
        {
            return await _context.Initiatives
                .Include(i => i.Organizer)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> ProcessDonationAsync(Donation donation, string? userId)
        {
            var initiative = await _context.Initiatives.FindAsync(donation.InitiativeId);
            if (initiative == null) return 0;

            if (userId != null)
            {
                donation.DonorId = userId;
            }

            donation.DonatedOn = DateTime.Now;

            _context.Donations.Add(donation);
            initiative.CurrentAmount += donation.Amount;

            await _context.SaveChangesAsync();
            return donation.Id;
        }

        public async Task<Donation?> GetDonationDetailsAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Initiative)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
