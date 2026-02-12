using System.ComponentModel.DataAnnotations;

namespace volunteerplatform.Models.ViewModels
{
    public class IndexViewModel
    {
        public string Username { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        // Volunteer Specifics
        public int? Age { get; set; }
        public string Skills { get; set; }
        public string Availability { get; set; }
        public string Location { get; set; }

        // Organizer Specifics
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }

        public string StatusMessage { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }
    }
}
