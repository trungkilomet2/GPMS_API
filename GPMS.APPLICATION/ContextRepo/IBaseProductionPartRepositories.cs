using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseProductionPartRepositories : IBaseRepositories<ProductionPart>
    {
        Task<IEnumerable<ProductionPart>> GetByProductionId(int productionId);
        Task<ProductionPart> GetDetailById(int partId);
        Task<IEnumerable<ProductionPart>> CreateMany(int productionId, IEnumerable<ProductionPart> parts);
        Task<ProductionPart> UpdatePart(ProductionPart part);
        Task<ProductionPart> AssignWorkers(int partId, IEnumerable<int> workerIds);
        Task DeletePart(int partId);
    }
}