using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateTemplateDTO
    {
        [Required(ErrorMessage = "TemplateName is required")]
        [StringLength(100, ErrorMessage = "TemplateName cannot exceed 100 characters")]
        public string TemplateName { get; set; }

        [StringLength(5, ErrorMessage = "Type cannot exceed 5 characters")]
        public string? Type { get; set; }

        [StringLength(255, ErrorMessage = "File path cannot exceed 255 characters")]
        public string? File { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}
