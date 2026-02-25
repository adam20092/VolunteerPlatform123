using Microsoft.AspNetCore.Identity;
using volunteerplatform.Models;
using volunteerplatform.Models.ViewModels;

namespace volunteerplatform.Services
{
    public interface IAccountService
    {
        Task<(IdentityResult Result, ApplicationUser? User)> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
    }
}
