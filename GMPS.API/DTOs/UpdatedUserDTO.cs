using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdatedUserDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100,
            ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15,
            ErrorMessage = "Phone number cannot exceed 15 digits")]
        public string PhoneNumber { get; set; }

        [Url(ErrorMessage = "Avatar URL must be a valid URL")]
        [StringLength(2048,
            ErrorMessage = "Avatar URL is too long")]
        public string AvartarUrl { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255,
            ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }
    }
}
