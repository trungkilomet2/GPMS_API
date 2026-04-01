using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateMaterialDTO
    {
        [Required(ErrorMessage = "MaterialName is required")]
        [StringLength(150, ErrorMessage = "MaterialName cannot exceed 150 characters")]
        public string MaterialName { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color { get; set; } = null!;

        [Url(ErrorMessage = "Image must be a valid URL")]
        [StringLength(255, ErrorMessage = "Image URL is too long")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        public decimal Value { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Uom is required")]
        [StringLength(50, ErrorMessage = "Uom cannot exceed 50 characters")]
        public string Uom { get; set; }
    }
}