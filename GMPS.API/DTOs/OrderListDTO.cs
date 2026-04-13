using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class OrderListDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OrderName { get; set; }
        public IEnumerable<OrderSize>? Size { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Image { get; set; }
        public string? Status { get; set; }
    }
}
