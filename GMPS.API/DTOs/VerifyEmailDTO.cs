using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}
