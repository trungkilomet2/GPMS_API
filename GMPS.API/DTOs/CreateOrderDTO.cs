using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Phải có Id người dùng")]
        [Range(1, int.MaxValue, ErrorMessage = "Id người dùng phải > 0")]
        public int UserId { get; set; }

        [Url(ErrorMessage = "Link ảnh không hợp lệ")]
        [StringLength(255, ErrorMessage = "Link ảnh quá dài")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên phải từ 3 đến 100 ký tự")]
        public string OrderName { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập kiểu")]
        [StringLength(50, ErrorMessage = "Kiểu không được vượt quá 5 ký tự")]
        public string Type { get; set; }

        [StringLength(5, ErrorMessage = "Kích thước không được vượt quá 5 ký tự")]
        [RegularExpression("^(XS|S|M|L|XL|XXL|XXXL)$",
    ErrorMessage = "Size must be XS, S, M, L, XL, XXL, or XXXL")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Yêu cầu chọn màu")]
        [StringLength(30, ErrorMessage = "Màu không được vượt quá 30 ký tự")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Phải có ngày bắt đầu")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Phải có ngày kết thức")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập số lượng")]
        [Range(1, 9999, ErrorMessage = "Số lượng phải từ 1 và không được vượt quá 9999")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá mỗi sản phẩm")]
        [Range(1000, 10000000, ErrorMessage = "Giá mỗi sản phẩm phải từ 1000 và không được vượt quá 10000000")]
        public decimal? Cpu { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Note { get; set; }
        public DateTime? CreateTime { get; set; }
        public List<CreateMaterialDTO>? Materials { get; set; }

        public List<CreateTemplateDTO>? Templates { get; set; }
    }
}
