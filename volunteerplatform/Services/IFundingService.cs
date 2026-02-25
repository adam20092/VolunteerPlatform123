using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public interface IFundingService
    {
        Task<Initiative?> GetInitiativeForDonationAsync(int id);
        Task<int> ProcessDonationAsync(Donation donation, string? userId);
        Task<Donation?> GetDonationDetailsAsync(int id);
    }
}
