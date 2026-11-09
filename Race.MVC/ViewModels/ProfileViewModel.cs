using System.ComponentModel.DataAnnotations;

namespace Race.MVC.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email can be at most 100 characters.")]
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;
    }
}
