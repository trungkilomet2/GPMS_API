using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateOrderDTO
    {
        [Required(ErrorMessage = "Order name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Order name must be between 3 and 100 characters")]
        public string OrderName { get; set; } = null!;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = null!;

        [StringLength(5, ErrorMessage = "Size cannot exceed 5 characters")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "StartDate is required")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        public IFormFile? Image { get; set; }

        [StringLength(255, ErrorMessage = "Note cannot exceed 255 characters")]
        public string? Note { get; set; }

        public List<UpdateTemplateDTO>? Templates { get; set; }

        public List<UpdateMaterialDTO>? Materials { get; set; }
    }

    public class UpdateTemplateDTO
    {
        [StringLength(100, ErrorMessage = "TemplateName cannot exceed 100 characters")]
        public string TemplateName { get; set; }

        [StringLength(5, ErrorMessage = "Type cannot exceed 5 characters")]
        public string? Type { get; set; }

        public IFormFile? File { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }
    }

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