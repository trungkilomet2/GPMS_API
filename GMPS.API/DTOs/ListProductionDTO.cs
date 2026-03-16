using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ListProductionDTO
    {
        public int ProductionId { get; set; }
        public int PmId { get; set; }
        public ListOrderProductionDTO Order { get; set; } = new() ;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
    }

    public class ListOrderProductionDTO
    {   
        public int Id { get; set; }
        public string OrderName { get; set; }
    }

}
