using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public interface ITeamService
    {
        Task<IEnumerable<Team>> GetAllTeamsAsync();
        Task<Team?> GetTeamByIdAsync(int id);
        Task<bool> JoinTeamAsync(int teamId, string userId);
        Task<bool> LeaveTeamAsync(int teamId, string userId);
        Task<Team> CreateTeamAsync(Team team, string leaderId);
    }

    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;

        public TeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams
                .Include(t => t.Leader)
                .Include(t => t.Members)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _context.Teams
                .Include(t => t.Leader)
                .Include(t => t.Members!).ThenInclude(m => m.Member)
                .Include(t => t.Initiatives)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> JoinTeamAsync(int teamId, string userId)
        {
            var alreadyMember = await _context.TeamMembers
                .AnyAsync(m => m.TeamId == teamId && m.MemberId == userId);

            if (alreadyMember) return false;

            var membership = new TeamMember
            {
                TeamId = teamId,
                MemberId = userId,
                JoinedAt = DateTime.Now,
                Role = "General Volunteer"
            };

            _context.TeamMembers.Add(membership);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveTeamAsync(int teamId, string userId)
        {
            var membership = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.MemberId == userId);

            if (membership == null) return false;

            _context.TeamMembers.Remove(membership);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Team> CreateTeamAsync(Team team, string leaderId)
        {
            team.LeaderId = leaderId;
            team.CreatedAt = DateTime.Now;
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            
            // Add leader as first member
            await JoinTeamAsync(team.Id, leaderId);
            
            return team;
        }
    }
}
