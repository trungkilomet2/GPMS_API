using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateGuest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id Phải là số dương.")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [StringLength(10, ErrorMessage = "Số điện thoại không được vượt quá 10 ký tự.")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Địa chỉ không được vượt quá 100 ký tự.")]
        public string Address { get; set; }
    }
}
