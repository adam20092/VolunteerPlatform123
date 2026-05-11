using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;

namespace VolunteerPlatform.Tests
{
    public class RatingServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SubmitRatingAsync_SavesRatingAndUpdatesVolunteerAverage()
        {
            // Arrange
            using var context = GetDbContext("SubmitRating");
            var volunteer = new ApplicationUser { Id = "vol-1", FullName = "Test Volunteer", Rating = 0 };
            context.Users.Add(volunteer);
            await context.SaveChangesAsync();

            var service = new RatingService(context);
            
            // Act 1: Submit first rating (5)
            var rating1 = new Rating { InitiativeId = 1, VolunteerId = "vol-1", Score = 5 };
            await service.SubmitRatingAsync(rating1, "org-1");

            // Assert 1
            var updatedVol = await context.Users.FindAsync("vol-1");
            Assert.Equal(5, updatedVol?.Rating);

            // Act 2: Submit second rating (3)
            var rating2 = new Rating { InitiativeId = 2, VolunteerId = "vol-1", Score = 3 };
            await service.SubmitRatingAsync(rating2, "org-1");

            // Assert 2
            // Average of 5 and 3 is 4
            Assert.Equal(4, updatedVol?.Rating);
        }

        [Fact]
        public async Task SubmitRatingAsync_UpdatesExistingRating()
        {
            // Arrange
            using var context = GetDbContext("UpdateRating");
            var volunteer = new ApplicationUser { Id = "vol-2", Rating = 0 };
            context.Users.Add(volunteer);
            await context.SaveChangesAsync();

            var service = new RatingService(context);

            // Act 1: Initial rating 5
            await service.SubmitRatingAsync(new Rating { InitiativeId = 10, VolunteerId = "vol-2", Score = 5 }, "org-1");
            
            // Act 2: Update same mission rating to 1
            await service.SubmitRatingAsync(new Rating { InitiativeId = 10, VolunteerId = "vol-2", Score = 1 }, "org-1");

            // Assert
            var updatedVol = await context.Users.FindAsync("vol-2");
            Assert.Equal(1, updatedVol?.Rating);
            Assert.Equal(1, await context.Ratings.CountAsync());
        }
    }
}
