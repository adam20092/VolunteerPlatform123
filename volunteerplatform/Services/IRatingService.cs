using volunteerplatform.Models;

namespace volunteerplatform.Services
{
    public interface IRatingService
    {
        Task<bool> SubmitRatingAsync(Rating rating, string organizerId);
    }
}
