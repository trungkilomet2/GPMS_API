using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateMaterialDTO
    {
        [Required(ErrorMessage = "MaterialName is required")]
        [StringLength(100, ErrorMessage = "MaterialName cannot exceed 100 characters")]
        public string MaterialName { get; set; }

        [Url(ErrorMessage = "Image must be a valid URL")]
        [StringLength(2048, ErrorMessage = "Image URL is too long")]
        public string? Image { get; set; }
        [Required(ErrorMessage = "Color is required")]
        [MaxLength(30, ErrorMessage = "Note cannot exceed 30 characters")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal Value { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Uom is required")]
        [StringLength(20, ErrorMessage = "Uom cannot exceed 20 characters")]
        public string Uom { get; set; }
    }
}
