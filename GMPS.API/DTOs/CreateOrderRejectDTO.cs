using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateOrderRejectDTO
    {
        [Required(ErrorMessage = "OrderId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "OrderId must be greater than 0")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Yêu cầu lý do")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Lý do phải từ 5 đến 150 ký tự")]
        public string? Reason { get; set; }

    }
}
