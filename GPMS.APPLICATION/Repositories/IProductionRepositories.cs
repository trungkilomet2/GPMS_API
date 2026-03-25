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

    }
}
