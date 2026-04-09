using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class VerifyEmailDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email sai định dạng")]
        public string? Email { get; set; }
    }
}
