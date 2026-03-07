using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class AddMaterialDTO
    {
        [Required(ErrorMessage = "Material name is required")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Material name must be between 1 and 150 characters")]
        public string Name { get; set; }

        [Url(ErrorMessage = "Image must be a valid URL")]
        [StringLength(255, ErrorMessage = "Image URL is too long")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]  
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Unit of measure is required")]
        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
        public string Uom { get; set; }

        [StringLength(100, ErrorMessage = "Note cannot exceed 100 characters")]
        public string? Note { get; set; }
    }
}