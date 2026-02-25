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

            rating.OrganizerId = organizerId;

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
