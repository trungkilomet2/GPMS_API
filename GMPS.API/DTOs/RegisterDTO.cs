using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{   

    /// <summary>
    /// -------Register------
    /// PHONENUMBER
    /// FULLNAME
    /// PASSWORD
    /// REPASSWORD
    /// ---------------------
    /// </summary>
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(11,ErrorMessage = "Số điện thoại có độ dài tối đa là 11")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        public string RePassword { get; set; }

    }
}
    