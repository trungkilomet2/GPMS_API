using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateSizeDTO
    {
        [Required(ErrorMessage = "Kích thước là bắt buộc")]
        public int SizeId { get; set; }

        [Required(ErrorMessage = "Màu là bắt buộc")]
        [StringLength(30, ErrorMessage = "Màu tối đa là 30 ký tự")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, 99999, ErrorMessage = "Số lượng phải từ 1 và không được vượt quá 99999")]
        public int? Quantity { get; set; }
    }
}
