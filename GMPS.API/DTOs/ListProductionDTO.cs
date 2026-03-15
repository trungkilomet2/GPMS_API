using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ListProductionDTO
    {
        public int Id { get; set; }
        public int PmId { get; set; }
        public IEnumerable<Order> Order { get; set; } = new List<Order>();
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
    }
}
