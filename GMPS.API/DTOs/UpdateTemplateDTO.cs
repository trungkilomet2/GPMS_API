using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateTemplateDTO
    {
        [StringLength(100, ErrorMessage = "TemplateName cannot exceed 100 characters")]
        public string TemplateName { get; set; }

        [StringLength(5, ErrorMessage = "Type cannot exceed 5 characters")]
        public string? Type { get; set; }

        [Url(ErrorMessage = "File must be a valid URL")]
        [StringLength(2048, ErrorMessage = "File URL is too long")]
        public string? File { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }
}