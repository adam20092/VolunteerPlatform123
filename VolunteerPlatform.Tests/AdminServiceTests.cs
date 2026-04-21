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
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager(IQueryable<ApplicationUser>? users = null)
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            
            if (users != null)
            {
                userManager.Setup(m => m.Users).Returns(users);
            }
            else
            {
                userManager.Setup(m => m.Users).Returns(new List<ApplicationUser>().AsQueryable());
            }

            return userManager;
        }

        [Fact]
        public async Task GetAllRequestsAsync_ReturnsAllEnrolments()
        {
            // Arrange
            var dbName = "GetAllRequests_" + Guid.NewGuid();
            using (var context = GetDbContext(dbName))
            {
                var initiative = new Initiative { Id = 1000, Title = "I1" };
                var volunteer = new ApplicationUser { Id = "v1", UserName = "v1" };
                context.Initiatives.Add(initiative);
                context.Users.Add(volunteer);

                context.Enrolments.Add(new Enrolment { Id = 1001, InitiativeId = 1000, VolunteerId = "v1", AppliedOn = DateTime.Now });
                context.Enrolments.Add(new Enrolment { Id = 1002, InitiativeId = 1000, VolunteerId = "v1", AppliedOn = DateTime.Now.AddDays(-1) });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var service = new AdminService(context, null!);

                // Act
                var result = await service.GetAllRequestsAsync();

                // Assert
                Assert.Equal(2, result.Count);
            }
        }

        [Fact]
        public async Task GetDashboardStatsAsync_CalculatesCorrectTotals()
        {
            // Arrange
            var dbName = "GetDashboardStats_" + Guid.NewGuid();
            using (var context = GetDbContext(dbName))
            {
                context.Initiatives.Add(new Initiative { Id = 1, Title = "M1", Status = MissionStatus.Active, DateAndTime = DateTime.Now });
                context.Initiatives.Add(new Initiative { Id = 2, Title = "M2", Status = MissionStatus.Finished, DateAndTime = DateTime.Now });
                context.Donations.Add(new Donation { Id = 1, Amount = 100 });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var userManagerMock = GetMockUserManager();
                userManagerMock.Setup(m => m.GetUsersInRoleAsync("Volunteer")).ReturnsAsync(new List<ApplicationUser> { new ApplicationUser() });
                userManagerMock.Setup(m => m.GetUsersInRoleAsync("Organizer")).ReturnsAsync(new List<ApplicationUser>());
                
                // For RecentUsers, we need a queryable that works with ToListAsync in InMemory
                userManagerMock.Setup(m => m.Users).Returns(context.Users);

                var service = new AdminService(context, userManagerMock.Object);

                // Act
                var stats = await service.GetDashboardStatsAsync();

                // Assert
                Assert.Equal(2, stats.TotalMission);
                Assert.Equal(1, stats.CompletedMissions);
                Assert.Equal(100, stats.TotalDonations);
            }
        }

        [Fact]
        public async Task DeleteUserAsync_CleansUpRelatedData()
        {
            // Arrange
            var dbName = "DeleteUserCleanup_" + Guid.NewGuid();
            var userId = "bad-user";
            using (var context = GetDbContext(dbName))
            {
                context.Enrolments.Add(new Enrolment { Id = 2001, VolunteerId = userId });
                context.Initiatives.Add(new Initiative { Id = 2002, Title = "Mission to Delete", OrganizerId = userId });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var userManagerMock = GetMockUserManager();
                var user = new ApplicationUser { Id = userId };
                userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
                userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

                var service = new AdminService(context, userManagerMock.Object);

                // Act
                var result = await service.DeleteUserAsync(userId);

                // Assert
                Assert.True(result);
            }
            
            using (var context = GetDbContext(dbName))
            {
                Assert.Equal(0, await context.Enrolments.CountAsync());
                Assert.Equal(0, await context.Initiatives.CountAsync());
            }
        }
    }
}
