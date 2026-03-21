using GPMS.DOMAIN.Entities;
using System.ComponentModel;

namespace GMPS.API.DTOs
{
    public class OrderRequestDTO : RequestDTO<Order>
    {
        [DefaultValue(null)]
        public string? Status { get; set; } = null;

        public DateOnly? StartDateFrom { get; set; } = null;

        public DateOnly? StartDateTo { get; set; } = null;
    }
}
