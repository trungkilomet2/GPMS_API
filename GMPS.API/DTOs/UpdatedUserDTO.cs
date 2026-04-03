using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdatedUserDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        [StringLength(50,
            ErrorMessage = "Tên không vượt quá 50 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(10,
            ErrorMessage = "Số điện thoại không vượt quá 15 số")]
        public string PhoneNumber { get; set; }
        public IFormFile? AvartarUrl { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập địa chỉ")]
        [StringLength(255,
    ErrorMessage = "Địa chỉ quá dài")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "email không hợp lệ")]
        [StringLength(255,
            ErrorMessage = "Email không quá 255 ký tự")]
        public string Email { get; set; }
    }
}
