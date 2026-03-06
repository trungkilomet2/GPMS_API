using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Url(ErrorMessage = "Image must be a valid URL")]
        [StringLength(2048, ErrorMessage = "Image URL is too long")]
        public string? Image { get; set; }

        [Required(ErrorMessage = "Order name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Order name must be between 3 and 50 characters")]
        public string OrderName { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(30, ErrorMessage = "Type cannot exceed 30 characters")]
        public string Type { get; set; }

        [StringLength(5, ErrorMessage = "Size cannot exceed 5 characters")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(10, ErrorMessage = "Color cannot exceed 10 characters")]
        public string Color { get; set; }

        [Required(ErrorMessage = "StartDate is required")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cpu must be greater than or equal to 0")]
        public decimal? Cpu { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }
}
