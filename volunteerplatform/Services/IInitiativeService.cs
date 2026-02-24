using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public interface IInitiativeService
    {
        Task<IEnumerable<Initiative>> GetAllInitiativesAsync(string? searchString = null);
        Task<IEnumerable<Initiative>> GetActiveInitiativesWithLocationAsync();
        Task<Initiative?> GetInitiativeByIdAsync(int id);
        Task<Initiative> CreateInitiativeAsync(Initiative initiative, string organizerId);
        Task<IEnumerable<Initiative>> GetInitiativesByOrganizerAsync(string organizerId);
        Task<bool> DeleteInitiativeAsync(int id, string userId, bool isAdmin);
        Task<bool> FinishInitiativeAsync(int id, string userId, bool isAdmin);
    }
}
