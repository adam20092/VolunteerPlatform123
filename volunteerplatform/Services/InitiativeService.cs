using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public class InitiativeService : IInitiativeService
    {
        private readonly ApplicationDbContext _context;

        public InitiativeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Initiative>> GetAllInitiativesAsync(string? searchString = null)
        {
            var initiatives = _context.Initiatives.Include(i => i.Organizer).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                initiatives = initiatives.Where(s => s.Title!.Contains(searchString) 
                                               || s.Location!.Contains(searchString)
                                               || s.Description!.Contains(searchString));
            }

            return await initiatives.ToListAsync();
        }

        public async Task<IEnumerable<Initiative>> GetActiveInitiativesWithLocationAsync()
        {
            return await _context.Initiatives
                .Where(i => i.Status == MissionStatus.Active && i.Latitude != null && i.Longitude != null)
                .ToListAsync();
        }

        public async Task<Initiative?> GetInitiativeByIdAsync(int id)
        {
            return await _context.Initiatives
                .Include(i => i.Organizer)
                .Include(i => i.Enrolments!).ThenInclude(e => e.Volunteer)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Initiative> CreateInitiativeAsync(Initiative initiative, string organizerId)
        {
            initiative.OrganizerId = organizerId;
            initiative.Status = MissionStatus.Active;
            _context.Add(initiative);
            await _context.SaveChangesAsync();
            return initiative;
        }

        public async Task<IEnumerable<Initiative>> GetInitiativesByOrganizerAsync(string organizerId)
        {
            return await _context.Initiatives
                .Where(i => i.OrganizerId == organizerId)
                .ToListAsync();
        }

        public async Task<bool> DeleteInitiativeAsync(int id, string userId, bool isAdmin)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return false;

            if (initiative.OrganizerId != userId && !isAdmin)
            {
                return false;
            }

            _context.Initiatives.Remove(initiative);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinishInitiativeAsync(int id, string userId, bool isAdmin)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return false;

            if (initiative.OrganizerId != userId && !isAdmin)
            {
                return false;
            }

            initiative.Status = MissionStatus.Finished;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
