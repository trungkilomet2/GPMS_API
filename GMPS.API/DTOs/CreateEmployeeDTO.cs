using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateEmployeeDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6 đến 50 ký tự")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 50 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không vượt quá 50 ký tự")]
        public string FullName { get; set; } = null!;
        public int ManagerId { get; set; }

        [Required(ErrorMessage = "Yêu cầu chọn vai trò")]
        [MinLength(1, ErrorMessage = "Chọn ít nhất 1 vai trò")]
        public List<int>? RoleIds { get; set; }
    }
}
