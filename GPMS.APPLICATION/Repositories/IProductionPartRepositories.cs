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
        Task DeletePart(int partId);

        // P_Part =============> Assignment    
        Task<ProductionPartDetailViewDTO> AssignWorkers(int partId, IEnumerable<int> workerIds);
        Task<ProductionPartDetailViewDTO> RemoveWorker(int partId, int workerId);
        // Lấy danh sách của worker có thể assign vào part đó gồm tên user + kĩ năng + lịch nghỉ phép
        Task<IEnumerable<AssignWorkerViewDTO>> ListAssignWorker(int pm_id);

    }
}