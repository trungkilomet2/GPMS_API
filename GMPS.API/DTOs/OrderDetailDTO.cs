using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class OrderDetailDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserPhone { get; set; }
        public string? UserLocation { get; set; }
        public string OrderName { get; set; }
        public string Type { get; set; }
        public string? Size { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Image { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public IEnumerable<OTemplate> Templates { get; set; } = new List<OTemplate>();
        public IEnumerable<OMaterial> Materials { get; set; } = new List<OMaterial>();
    }
}