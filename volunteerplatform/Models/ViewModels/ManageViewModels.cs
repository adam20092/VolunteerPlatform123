using System.ComponentModel.DataAnnotations;

namespace volunteerplatform.Models.ViewModels
{
    public class IndexViewModel
    {
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Volunteer Specifics
        public int? Age { get; set; }
        public string Skills { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Organizer Specifics
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        public string StatusMessage { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string StatusMessage { get; set; } = string.Empty;
    }
}
