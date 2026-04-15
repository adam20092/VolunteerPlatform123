using Microsoft.EntityFrameworkCore;
using Moq;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;

namespace VolunteerPlatform.Tests
{
    public class EnrolmentServiceTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task ApplyAsync_Succeeds_WhenNew()
        {
            // Arrange
            var options = GetDbOptions("ApplyAsync_Succeeds");
            var mockEmail = new Mock<IEmailService>();
            
            using (var context = new ApplicationDbContext(options))
            {
                var service = new EnrolmentService(context, mockEmail.Object);
                
                // Act
                var result = await service.ApplyAsync(1, "test-user-id");

                // Assert
                Assert.True(result);
                Assert.Equal(1, await context.Enrolments.CountAsync());
            }
        }

        [Fact]
        public async Task ApplyAsync_Fails_WhenDuplicate()
        {
            // Arrange
            var options = GetDbOptions("ApplyAsync_Fails");
            var mockEmail = new Mock<IEmailService>();
            
            using (var context = new ApplicationDbContext(options))
            {
                context.Enrolments.Add(new Enrolment { InitiativeId = 1, VolunteerId = "user1" });
                await context.SaveChangesAsync();

                var service = new EnrolmentService(context, mockEmail.Object);
                
                // Act
                var result = await service.ApplyAsync(1, "user1");

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public async Task UpdateStatusAsync_SendsEmail_OnApproval()
        {
            // Arrange
            var options = GetDbOptions("UpdateStatusEmail");
            var mockEmail = new Mock<IEmailService>();
            
            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "v1", Email = "v1@test.com", FullName = "Vanya" };
                var initiative = new Initiative { Id = 10, Title = "Test Mission" };
                var enrolment = new Enrolment 
                { 
                    Id = 100, 
                    InitiativeId = 10, 
                    VolunteerId = "v1", 
                    Status = EnrolmentStatus.Pending 
                };

                context.Users.Add(user);
                context.Initiatives.Add(initiative);
                context.Enrolments.Add(enrolment);
                await context.SaveChangesAsync();

                var service = new EnrolmentService(context, mockEmail.Object);

                // Act
                await service.UpdateStatusAsync(100, EnrolmentStatus.Approved);

                // Assert
                mockEmail.Verify(e => e.SendVolunteerApprovalEmailAsync(
                    "v1@test.com", "Vanya", "Test Mission"), Times.Once);
            }
        }
    }
}
