using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(250)]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
