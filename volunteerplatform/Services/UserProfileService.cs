using Microsoft.AspNetCore.Identity;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserProfileService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IndexViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new IndexViewModel
            {
                Username = user.UserName ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Age = user.Age,
                Skills = user.Skills ?? string.Empty,
                Availability = user.Availability ?? string.Empty,
                Location = user.Location ?? string.Empty,
                OrganizationName = user.OrganizationName ?? string.Empty
            };
        }

        public async Task<IdentityResult> UpdateProfileAsync(string userId, IndexViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Age = model.Age;
            user.Skills = model.Skills;
            user.Availability = model.Availability;
            user.Location = model.Location;
            user.OrganizationName = model.OrganizationName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            return result;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            return result;
        }
    }
}
