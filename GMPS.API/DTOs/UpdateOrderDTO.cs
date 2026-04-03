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
        [RegularExpression("^(XS|S|M|L|XL|XXL|XXXL)$", ErrorMessage = "Size must be XS, S, M, L, XL, XXL, or XXXL")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "StartDate is required")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 9999, ErrorMessage = "Quantity must be between 1 and 9999")]
        public int Quantity { get; set; }

        [Url(ErrorMessage = "Image must be a valid URL")]
        [StringLength(255, ErrorMessage = "Image URL is too long")]
        public string? Image { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }

        public List<UpdateTemplateDTO>? Templates { get; set; }

        public List<UpdateMaterialDTO>? Materials { get; set; }
    }
}