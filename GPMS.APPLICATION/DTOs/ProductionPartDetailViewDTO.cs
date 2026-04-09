using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.DTOs
{
    public class ProductionPartDetailViewDTO
    {
        public ProductionPart Part { get; set; } = new();
        public IEnumerable<ProductionPartOrderSize> ListPartOrderSize { get; set; } = new List<ProductionPartOrderSize>();

    }
}