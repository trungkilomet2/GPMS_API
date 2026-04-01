using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateMaterialDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập tên nguyên liệu")]
        [StringLength(150, ErrorMessage = "Tên nguyên liệu không vượt quá 150 ký tự")]
        public string MaterialName { get; set; }

        [Url(ErrorMessage = "Link ảnh không hợp lệ")]
        [StringLength(255, ErrorMessage = "Link ảnh quá dài")]
        public string? Image { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập màu")]
        [MaxLength(30, ErrorMessage = "Màu không được vượt quá 30 ký tự")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số lượng")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public decimal Value { get; set; }

        [MaxLength(100, ErrorMessage = "Ghi chú không vượt quá 100 ký tự")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập đơn vị")]
        [StringLength(50, ErrorMessage = "Đơn vị không vượt quá 50 ký tự")]
        public string Uom { get; set; }
    }
}
