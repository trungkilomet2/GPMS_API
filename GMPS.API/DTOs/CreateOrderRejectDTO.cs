using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateOrderRejectDTO
    {
        [Required(ErrorMessage = "OrderId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "OrderId must be greater than 0")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
        public string? Reason { get; set; }

    }
}
