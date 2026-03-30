using Microsoft.AspNetCore.Identity;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public interface IUserProfileService
    {
        Task<IndexViewModel?> GetProfileAsync(string userId);
        Task<IdentityResult> UpdateProfileAsync(string userId, IndexViewModel model);
        Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<byte[]> GetPersonalDataAsync(string userId);
        Task<bool> DeleteAccountAsync(string userId);
    }
}
