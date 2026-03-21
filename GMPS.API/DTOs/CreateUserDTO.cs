using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "UserName is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "UserName must be between 6 and 50 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "FullName is required")]
        [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "At least one role must be assigned")]
        [MinLength(1, ErrorMessage = "At least one role must be provided")]
        public List<int> RoleIds { get; set; } = new();
    }
}
