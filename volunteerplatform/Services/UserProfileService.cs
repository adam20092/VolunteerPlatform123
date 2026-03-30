using Microsoft.AspNetCore.Identity;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAdminService _adminService;
        private readonly Data.ApplicationDbContext _context;

        public UserProfileService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAdminService adminService,
            Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _adminService = adminService;
            _context = context;
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

        public async Task<byte[]> GetPersonalDataAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Array.Empty<byte>();

            var personalData = new Dictionary<string, string>
            {
                { "UserId", user.Id },
                { "UserName", user.UserName ?? "" },
                { "Email", user.Email ?? "" },
                { "FullName", user.FullName ?? "" },
                { "PhoneNumber", user.PhoneNumber ?? "" },
                { "Age", user.Age?.ToString() ?? "" },
                { "Location", user.Location ?? "" },
                { "Skills", user.Skills ?? "" },
                { "OrganizationName", user.OrganizationName ?? "" }
            };

            // Add some activity stats
            var missionsCompleted = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(_context.Enrolments.Where(e => e.VolunteerId == userId && e.Status == EnrolmentStatus.Approved));
            personalData.Add("MissionsCompleted", missionsCompleted.ToString());

            var json = System.Text.Json.JsonSerializer.Serialize(personalData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<bool> DeleteAccountAsync(string userId)
        {
            var success = await _adminService.DeleteUserAsync(userId);
            if (success)
            {
                await _signInManager.SignOutAsync();
            }
            return success;
        }
    }
}
