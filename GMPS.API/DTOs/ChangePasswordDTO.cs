using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
