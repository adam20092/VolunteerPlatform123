using Microsoft.EntityFrameworkCore;
using Moq;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VolunteerPlatform.Tests
{
    public class InitiativeServiceTests
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
        public async Task CreateInitiativeAsync_SetsOrganizerAndStatus()
        {
            // Arrange
            using var context = GetDbContext();
            var emailServiceMock = new Mock<IEmailService>();
            var userManagerMock = GetMockUserManager();
            var service = new InitiativeService(context, emailServiceMock.Object, userManagerMock.Object);
            
            var initiative = new Initiative { Title = "Test Mission", Location = "Sofia" };
            var organizerId = "org-1";

            userManagerMock.Setup(m => m.GetUsersInRoleAsync("Volunteer"))
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await service.CreateInitiativeAsync(initiative, organizerId);

            // Assert
            Assert.Equal(organizerId, result.OrganizerId);
            Assert.Equal(MissionStatus.Active, result.Status);
            Assert.Equal(1, await context.Initiatives.CountAsync());
        }

        [Fact]
        public async Task GetInitiativeByIdAsync_ReturnsCorrectInitiative()
        {
            // Arrange
            using var context = GetDbContext();
            var initiative = new Initiative { Id = 1, Title = "Mission 1" };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();

            var service = new InitiativeService(context, null, null);

            // Act
            var result = await service.GetInitiativeByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Mission 1", result.Title);
        }

        [Fact]
        public async Task DeleteInitiativeAsync_RemovesFromDatabase()
        {
            // Arrange
            using var context = GetDbContext();
            var initiative = new Initiative { Id = 10, OrganizerId = "user-1" };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();

            var service = new InitiativeService(context, null, null);

            // Act
            var deleted = await service.DeleteInitiativeAsync(10, "user-1", false);

            // Assert
            Assert.True(deleted);
            Assert.Equal(0, await context.Initiatives.CountAsync());
        }

        [Fact]
        public async Task FinishInitiativeAsync_UpdatesStatus()
        {
            // Arrange
            using var context = GetDbContext();
            var initiative = new Initiative { Id = 5, OrganizerId = "admin", Status = MissionStatus.Active };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();

            var service = new InitiativeService(context, null, null);

            // Act
            await service.FinishInitiativeAsync(5, "admin", true);

            // Assert
            var updated = await context.Initiatives.FindAsync(5);
            Assert.Equal(MissionStatus.Finished, updated?.Status);
        }
    }
}
