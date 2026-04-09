using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6 đến 50 ký tự")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Phải có ít nhất một vai trò")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một vai trò")]
        public List<int> RoleIds { get; set; } = new();
    }
}
