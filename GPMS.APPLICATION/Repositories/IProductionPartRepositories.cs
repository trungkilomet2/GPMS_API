using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface IProductionPartRepositories
    {   
        // Lấy danh sách Part trong một Production.
        Task<IEnumerable<ProductionPartDetailViewDTO>> GetPartsByProductionId(int productionId);
        Task<ProductionPartDetailViewDTO> GetPartAssignmentDetail(int partId);
        Task<IEnumerable<ProductionPartDetailViewDTO>> CreateParts(int productionId, IEnumerable<ProductionPart> parts);
        Task<ProductionPartDetailViewDTO> UpdatePart(int partId, ProductionPart part);
        Task DeletePart(int partId);

        // P_Part =============> Assignment    
        Task<ProductionPartDetailViewDTO> AssignWorkers(int partId,int partOrderSizeId, IEnumerable<int> workerIds);
        Task<ProductionPartDetailViewDTO> RemoveWorker(int partId, int workerId);
        // Lấy danh sách của worker có thể assign vào part đó gồm tên user + kĩ năng + lịch nghỉ phép
        Task<IEnumerable<AssignWorkerViewDTO>> ListAssignWorker(int pm_id, DateTime fromDate, DateTime toDate);

        // Worker Log Work ========> Worker Log Work
        Task<IEnumerable<ProductionPartWorkLog>> GetWorkLogs(int partId);
        Task<ProductionPartWorkLog> CreateWorkLog(int partId,int partOrderSizeId, int userId, int quantity);
        Task<ProductionPartWorkLog> UpdateWorkLog(int partId,int partOrderSizeId, int workLogId, int quantity);
        // TrungNT 29-03-2026
        Task<ProductionPartDetailViewDTO> DoneAPart(int partId);
        Task<IEnumerable<User>> GetIssueWorkersByWorkLogs(int partId);

        Task<PartPaymentCompletionViewDTO> CompletePartPayment(int partId,int partOrderSizeId, IEnumerable<int> workLogIds);

        Task<ProductionPartCompletionEstimateViewDTO> EstimatePartCompletion(int partId, IEnumerable<int> workerIds);
        Task<IEnumerable<ProductionWorkerProgressChartViewDTO>> GetProductionWorkerProgressChart(int productionId);
        Task<IEnumerable<WorkerProductivityScoreViewDTO>> GetWorkerProductivityScores(int productionId);

    }
}