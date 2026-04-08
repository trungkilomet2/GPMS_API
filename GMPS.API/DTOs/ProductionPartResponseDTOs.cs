using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ProductionPartDetailDTO
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cpu { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public IEnumerable<ProductionPartOrderSize> ListPartOrderSizes { get; set; } = new List<ProductionPartOrderSize>();
    }
}
