using Microsoft.EntityFrameworkCore;
using volunteerplatform.Data;
using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SubmitRatingAsync(Rating rating, string organizerId)
        {
            if (rating.Score < 1 || rating.Score > 5)
                return false;

            // Check if rating already exists for this mission/volunteer
            var existing = await _context.Ratings
                .FirstOrDefaultAsync(r => r.InitiativeId == rating.InitiativeId && r.VolunteerId == rating.VolunteerId);

            if (existing != null)
            {
                existing.Score = rating.Score;
                existing.Comment = rating.Comment;
                existing.OrganizerId = organizerId;
            }
            else
            {
                rating.OrganizerId = organizerId;
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();

            // Update average rating for the volunteer
            var volunteer = await _context.Users.FindAsync(rating.VolunteerId);
            if (volunteer != null)
            {
                var ratings = await _context.Ratings
                    .Where(r => r.VolunteerId == rating.VolunteerId)
                    .Select(r => r.Score)
                    .ToListAsync();

                if (ratings.Any())
                {
                    volunteer.Rating = (int)Math.Round(ratings.Average());
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}
