using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ProductionDetailDTO
    {
        public int Id { get; set; } 
        public User PmId { get; set; }
        public Order OrderId { get; set; }
        public int ? StartDate { get; set; }
        public int  ? EndDate { get; set; }
        public int StatusId { get; set; }
    }

    public class ProductionPMDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ProductionOrderDTO
    {
        public int Id { get; set; }
        public string OrderName { get; set; } = string.Empty;
    }




}
