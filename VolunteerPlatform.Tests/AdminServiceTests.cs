using Microsoft.EntityFrameworkCore;
using Moq;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace VolunteerPlatform.Tests
{
    public class AdminServiceTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetAllRequestsAsync_ReturnsAllEnrolments()
        {
            // Arrange
            using var context = GetDbContext();
            context.Enrolments.Add(new Enrolment { Id = 1, AppliedOn = DateTime.Now });
            context.Enrolments.Add(new Enrolment { Id = 2, AppliedOn = DateTime.Now.AddDays(-1) });
            await context.SaveChangesAsync();

            var service = new AdminService(context, null);

            // Act
            var result = await service.GetAllRequestsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_CalculatesCorrectTotals()
        {
            // Arrange
            using var context = GetDbContext();
            context.Initiatives.Add(new Initiative { Id = 1, Status = MissionStatus.Active, DateAndTime = DateTime.Now });
            context.Initiatives.Add(new Initiative { Id = 2, Status = MissionStatus.Finished, DateAndTime = DateTime.Now });
            context.Donations.Add(new Donation { Id = 1, Amount = 100 });
            context.Donations.Add(new Donation { Id = 2, Amount = 50 });
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(m => m.GetUsersInRoleAsync("Volunteer")).ReturnsAsync(new List<ApplicationUser> { new ApplicationUser(), new ApplicationUser() });
            userManagerMock.Setup(m => m.GetUsersInRoleAsync("Organizer")).ReturnsAsync(new List<ApplicationUser> { new ApplicationUser() });
            userManagerMock.Setup(m => m.Users).Returns(new List<ApplicationUser>().AsQueryable());

            var service = new AdminService(context, userManagerMock.Object);

            // Act
            var stats = await service.GetDashboardStatsAsync();

            // Assert
            Assert.Equal(2, stats.TotalMission);
            Assert.Equal(1, stats.CompletedMissions);
            Assert.Equal(150, stats.TotalDonations);
            Assert.Equal(2, stats.TotalVolunteers);
        }

        [Fact]
        public async Task DeleteUserAsync_CleansUpRelatedData()
        {
            // Arrange
            using var context = GetDbContext();
            var userId = "bad-user";
            context.Enrolments.Add(new Enrolment { Id = 1, VolunteerId = userId });
            context.Initiatives.Add(new Initiative { Id = 1, OrganizerId = userId });
            await context.SaveChangesAsync();

            var userManagerMock = GetMockUserManager();
            var user = new ApplicationUser { Id = userId };
            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var service = new AdminService(context, userManagerMock.Object);

            // Act
            var result = await service.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            Assert.Equal(0, await context.Enrolments.CountAsync());
            Assert.Equal(0, await context.Initiatives.CountAsync());
        }
    }
}
