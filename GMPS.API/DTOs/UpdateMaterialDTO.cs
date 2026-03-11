using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateMaterialDTO
    {
        [Required(ErrorMessage = "MaterialName is required")]
        [StringLength(100, ErrorMessage = "MaterialName cannot exceed 100 characters")]
        public string MaterialName { get; set; }

        public IFormFile? Image { get; set; }

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