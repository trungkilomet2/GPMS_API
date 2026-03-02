using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{   

    /// <summary>
    /// -------Register------
    /// USERNAME
    /// FULLNAME
    /// PASSWORD
    /// REPASSWORD
    /// ---------------------
    /// </summary>
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        [StringLength(50,ErrorMessage = "Số điện thoại có độ dài tối đa là 50")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        public string RePassword { get; set; }

    }
}
    