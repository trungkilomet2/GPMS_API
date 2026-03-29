using GPMS.APPLICATION.DTOs;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IProductionRepositories
    {
        Task<Production> CreateProduction(Production production);
        // Change DTO -1
        Task<ProductionDetailViewDTO> GetProductionDetail(int productionId);
        Task<Production> RequestProductionRevision(int productionId);
        // Deny đang chờ để xem set trong nghiệp vụ hệ thống
        
        //Task<Production> DenyProduction(int productionId, int userId, string reason);
        Task<Production> UpdatePMProduction(int production_id, int new_pm_id);
        //The new DTO
        Task<ProductionDetailViewDTO> GetProductionDetailView(int productionId);
        Task<IEnumerable<ProductionDetailViewDTO>> GetProductionListViews();

        // Lacking Bussiness Logic -1
        // TrungNT - 22-03-26
        Task<Production> ApproveProduction(int productionId);
        Task<Production> RejectProduction(int productionId, string reason);
        Task<IEnumerable<ProductionIssueLog>> GetProductionIssues(int productionId);
        Task<IEnumerable<ProductionIssueLog>> GetProductionIssueSummaryByType(int productionId);
        Task<ProductionIssueLog> CreateProductionIssue(ProductionIssueLog issue);


        // TrungNT - 25-03-25
        // Chấp Nhận - Duyệt Kế Hoạch Từ Production
        Task<Production> ApproveProductionPlan(int productionId);
        
        // Cần Cập Nhật Thêm Production
        Task<Production> NeedUpdateProductionPlan(int productionId);
        // Lấy thông tin từ chối của Production đấy
        Task<ProductionRejectReason> ProductionRejectReasonDetail(int productionId);

        // 26-03-2026: Nghiệp vụ thống kê sản lượng / output
        Task<IEnumerable<ProductionWorkerOutputViewDTO>> GetProductionWorkerOutput(int productionId);
        Task<IEnumerable<WorkerProductivityHistoryViewDTO>> GetAllWorkersProductivityHistory();
        Task<ProductionOutputSummaryViewDTO> GetProductionOutputSummary(int productionId);
        Task<IEnumerable<WorkerProductivityHistoryViewDTO>> GetWorkerProductivityHistory(int workerId);
        Task<IEnumerable<WorkerAssignedPlanViewDTO>> GetWorkerAssignedPlans(int workerId);

        // Trung NT - 2026-03-30: Nghiệp vụ Hoàn thành Production
        Task<Production> CompleteProduction(int productionId);


    }
}
