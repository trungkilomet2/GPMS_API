using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class VerifyOtpDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email sai định dạng")]
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
