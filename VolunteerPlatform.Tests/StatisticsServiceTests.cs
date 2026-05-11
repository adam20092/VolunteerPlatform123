using Microsoft.EntityFrameworkCore;
using Moq;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace VolunteerPlatform.Tests
{
    public class StatisticsServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task GetHomeStatsAsync_ReturnsCorrectCounts()
        {
            // Arrange
            using var context = GetDbContext("HomeStats");
            context.Initiatives.Add(new Initiative { Title = "M1", Status = MissionStatus.Finished });
            context.Initiatives.Add(new Initiative { Title = "M2", Status = MissionStatus.Active });
            context.Donations.Add(new Donation { Amount = 10 });
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            // Mocking GetUsersInRoleAsync is tricky, but let's assume it returns a list
            userManagerMock.Setup(m => m.GetUsersInRoleAsync("Volunteer"))
                .ReturnsAsync(new List<ApplicationUser> { new ApplicationUser(), new ApplicationUser() });

            var service = new StatisticsService(context, userManagerMock.Object);

            // Act
            var stats = await service.GetHomeStatsAsync();

            // Assert
            Assert.Equal(2, stats.TotalInitiatives);
            Assert.Equal(2, stats.TotalVolunteers);
            Assert.Equal(1, stats.CompletedProjects);
        }

        [Fact]
        public async Task GetLeaderboardAsync_CalculatesPointsCorrectly()
        {
            // Arrange
            using var context = GetDbContext("Leaderboard");
            var v1 = new ApplicationUser { Id = "v1", FullName = "V1", Rating = 5 }; // 5*10 = 50 pts
            var v2 = new ApplicationUser { Id = "v2", FullName = "V2", Rating = 3 }; // 3*10 = 30 pts
            context.Users.AddRange(v1, v2);
            
            // Add roles manually to DB for leaderboard query
            context.UserRoles.Add(new IdentityUserRole<string> { UserId = "v1", RoleId = "r-vol" });
            context.UserRoles.Add(new IdentityUserRole<string> { UserId = "v2", RoleId = "r-vol" });
            context.Roles.Add(new IdentityRole { Id = "r-vol", Name = "Volunteer" });

            // V2 has 1 approved mission (+100 pts)
            context.Enrolments.Add(new Enrolment { VolunteerId = "v2", Status = EnrolmentStatus.Approved });
            
            await context.SaveChangesAsync();

            var service = new StatisticsService(context, null!);

            // Act
            var leaderboard = await service.GetLeaderboardAsync();

            // Assert
            // V2: 100 + 30 = 130
            // V1: 0 + 50 = 50
            Assert.Equal("V2", leaderboard[0].FullName);
            Assert.Equal(130, leaderboard[0].TotalPoints);
            Assert.Equal("V1", leaderboard[1].FullName);
            Assert.Equal(50, leaderboard[1].TotalPoints);
        }
    }
}
