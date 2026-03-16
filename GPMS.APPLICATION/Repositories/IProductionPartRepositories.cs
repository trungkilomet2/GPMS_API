using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface IProductionPartRepositories
    {
        Task<IEnumerable<ProductionPartDetailViewDTO>> GetPartsByProductionId(int productionId);
        Task<ProductionPartDetailViewDTO> GetPartAssignmentDetail(int partId);
        Task<IEnumerable<ProductionPartDetailViewDTO>> CreateParts(int productionId, IEnumerable<ProductionPart> parts);
        Task<ProductionPartDetailViewDTO> UpdatePart(int partId, ProductionPart part);
        Task<ProductionPartDetailViewDTO> AssignWorkers(int partId, IEnumerable<int> workerIds);
        Task DeletePart(int partId);
    }
}