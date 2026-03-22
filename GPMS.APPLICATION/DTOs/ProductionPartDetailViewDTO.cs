using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.DTOs
{
    public class ProductionPartDetailViewDTO
    {
        public ProductionPart Part { get; set; } = new();
        public IEnumerable<User> Assignees { get; set; } = new List<User>();

    }
}