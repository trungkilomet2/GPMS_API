using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateTemplateDTO
    {
        [Required(ErrorMessage = "TemplateName is required")]
        [StringLength(100, ErrorMessage = "TemplateName cannot exceed 100 characters")]
        public string TemplateName { get; set; }

        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string? Type { get; set; }

        [StringLength(255, ErrorMessage = "File path cannot exceed 255 characters")]
        public string? File { get; set; }

        [MaxLength(100, ErrorMessage = "Note cannot exceed 100 characters")]
        public string? Note { get; set; }
    }
}
