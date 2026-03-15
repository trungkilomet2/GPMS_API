using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ListProductionDTO
    {
        public int Id { get; set; }
        public int PmId { get; set; }
        public Order Order { get; set; } = new() ;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
    }
}
