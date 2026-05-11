using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;

namespace VolunteerPlatform.Tests
{
    public class TeamServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateTeamAsync_SetsLeaderAndAddsMember()
        {
            // Arrange
            using var context = GetDbContext("CreateTeam");
            var service = new TeamService(context);
            var team = new Team { Name = "Test Team" };
            var leaderId = "leader-1";

            // Act
            var result = await service.CreateTeamAsync(team, leaderId);

            // Assert
            Assert.Equal(leaderId, result.LeaderId);
            Assert.Equal(1, await context.Teams.CountAsync());
            Assert.Equal(1, await context.TeamMembers.CountAsync(m => m.TeamId == result.Id && m.MemberId == leaderId));
        }

        [Fact]
        public async Task JoinTeamAsync_PreventsDuplicateMembership()
        {
            // Arrange
            using var context = GetDbContext("JoinTeamDuplicate");
            var service = new TeamService(context);
            var team = new Team { Id = 1, Name = "Team 1" };
            context.Teams.Add(team);
            await context.SaveChangesAsync();

            // Act 1: First join
            var success1 = await service.JoinTeamAsync(1, "user-1");
            // Act 2: Second join
            var success2 = await service.JoinTeamAsync(1, "user-1");

            // Assert
            Assert.True(success1);
            Assert.False(success2);
            Assert.Equal(1, await context.TeamMembers.CountAsync(m => m.TeamId == 1 && m.MemberId == "user-1"));
        }

        [Fact]
        public async Task LeaveTeamAsync_RemovesMembership()
        {
            // Arrange
            using var context = GetDbContext("LeaveTeam");
            var service = new TeamService(context);
            context.TeamMembers.Add(new TeamMember { TeamId = 2, MemberId = "user-2" });
            await context.SaveChangesAsync();

            // Act
            var result = await service.LeaveTeamAsync(2, "user-2");

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.TeamMembers.CountAsync());
        }
    }
}
