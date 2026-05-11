using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;
using volunteerplatform.Services;
using Xunit;

namespace VolunteerPlatform.Tests
{
    public class AchievementServiceTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAchievementsAsync_CalculatesCorrectStatus()
        {
            // Arrange
            using var context = GetDbContext("AchievementStats");
            var userId = "user-ach-1";
            var user = new ApplicationUser { Id = userId, Rating = 5 };
            context.Users.Add(user);

            // 1 mission completed
            context.Enrolments.Add(new Enrolment { VolunteerId = userId, Status = EnrolmentStatus.Approved, Initiative = new Initiative { Title = "Mission 1", Location = "Sofia" } });
            
            // 1 donation
            context.Donations.Add(new Donation { DonorId = userId, Amount = 100 });

            await context.SaveChangesAsync();

            var service = new AchievementService(context);

            // Act
            var result = await service.GetAchievementsAsync(userId);

            // Assert
            // Points: (1 mission * 100) + (5 rating * 10) + (1 donation * 50) = 100 + 50 + 50 = 200
            Assert.Equal(200, result.TotalPoints);
            Assert.Equal("Rising Star", result.OverallBadge);
            
            // Check specific achievements
            var firstSteps = result.Achievements.First(a => a.Key == "first_steps");
            Assert.True(firstSteps.Unlocked);

            var topRated = result.Achievements.First(a => a.Key == "top_rated");
            Assert.True(topRated.Unlocked);

            var generousHeart = result.Achievements.First(a => a.Key == "generous_heart");
            Assert.True(generousHeart.Unlocked);

            var veteran = result.Achievements.First(a => a.Key == "veteran");
            Assert.False(veteran.Unlocked);
        }

        [Fact]
        public async Task GetAchievementsAsync_ExplorerCalculation()
        {
            // Arrange
            using var context = GetDbContext("ExplorerStats");
            var userId = "user-exp";
            
            // 3 missions in different locations
            context.Enrolments.Add(new Enrolment { VolunteerId = userId, Status = EnrolmentStatus.Approved, Initiative = new Initiative { Title = "Loc Mission 1", Location = "Sofia" } });
            context.Enrolments.Add(new Enrolment { VolunteerId = userId, Status = EnrolmentStatus.Approved, Initiative = new Initiative { Title = "Loc Mission 2", Location = "Plovdiv" } });
            context.Enrolments.Add(new Enrolment { VolunteerId = userId, Status = EnrolmentStatus.Approved, Initiative = new Initiative { Title = "Loc Mission 3", Location = "Varna" } });

            await context.SaveChangesAsync();

            var service = new AchievementService(context);

            // Act
            var result = await service.GetAchievementsAsync(userId);

            // Assert
            var explorer = result.Achievements.First(a => a.Key == "explorer");
            Assert.True(explorer.Unlocked);
            Assert.Equal(3, explorer.Progress);

            var globeTrotter = result.Achievements.First(a => a.Key == "globe_trotter");
            Assert.False(globeTrotter.Unlocked);
            Assert.Equal(3, globeTrotter.Progress);
        }
    }
}
