using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateTemplateDTO
    {
        [Required(ErrorMessage = "Yêu cập nhập tên mẫu")]
        [StringLength(100, ErrorMessage = "Tên mẫu không vượt quá 100 ký tự")]
        public string TemplateName { get; set; }

        [StringLength(50, ErrorMessage = "Tên kiểu không vượt quá 50 ký tự")]
        public string? Type { get; set; }

        [StringLength(255, ErrorMessage = "Tên file không được vượt quá 255 ký tự")]
        public string? File { get; set; }

        [MaxLength(100, ErrorMessage = "Ghi chú không được vượt quá 100 ký tự")]
        public string? Note { get; set; }
    }
}
