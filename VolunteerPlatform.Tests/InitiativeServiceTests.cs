using Microsoft.EntityFrameworkCore;
using Moq;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace VolunteerPlatform.Tests
{
    public class InitiativeServiceTests
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
        public async Task CreateInitiativeAsync_SetsOrganizerAndStatus()
        {
            // Arrange
            using var context = GetDbContext("CreateInitiative");
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
            using var context = GetDbContext("GetById");
            var initiative = new Initiative { Id = 301, Title = "Mission 301" };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();

            var service = new InitiativeService(context, null!, null!);

            // Act
            var result = await service.GetInitiativeByIdAsync(301);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Mission 301", result.Title);
        }

        [Fact]
        public async Task DeleteInitiativeAsync_RemovesFromDatabase()
        {
            // Arrange
            using var context = GetDbContext("DeleteInitiative");
            var initiative = new Initiative { Id = 401, Title = "D-Mission", OrganizerId = "user-1" };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();

            var service = new InitiativeService(context, null!, null!);

            // Act
            var deleted = await service.DeleteInitiativeAsync(401, "user-1", false);

            // Assert
            Assert.True(deleted);
            Assert.Equal(0, await context.Initiatives.CountAsync());
        }

        [Fact]
        public async Task ToggleFilledStatusAsync_TogglesBetweenActiveAndFilled()
        {
            // Arrange
            using var context = GetDbContext("ToggleStatus");
            var initiative = new Initiative { Id = 601, Title = "Status Toggle Mission", OrganizerId = "org-1", Status = MissionStatus.Active };
            context.Initiatives.Add(initiative);
            await context.SaveChangesAsync();
            var service = new InitiativeService(context, null!, null!);

            // Act 1: Active -> Filled
            await service.ToggleFilledStatusAsync(601, "org-1", false);
            // Assert 1
            Assert.Equal(MissionStatus.Filled, initiative.Status);

            // Act 2: Filled -> Active
            await service.ToggleFilledStatusAsync(601, "org-1", false);
            // Assert 2
            Assert.Equal(MissionStatus.Active, initiative.Status);
        }

        [Fact]
        public async Task GetAllInitiativesAsync_BilingualSearch_ReturnsCorrectResults()
        {
            // Arrange
            using var context = GetDbContext("BilingualSearch");
            context.Initiatives.Add(new Initiative { Title = "Clean the Park", Category = "Environment" });
            context.Initiatives.Add(new Initiative { Title = "Social Work", Category = "Social" });
            await context.SaveChangesAsync();
            var service = new InitiativeService(context, null!, null!);

            // Act: Search with Bulgarian word for Environment
            var results = await service.GetAllInitiativesAsync(searchString: "Околна среда");

            // Assert
            Assert.Single(results);
            Assert.Equal("Environment", results.First().Category);
        }
    }
}
