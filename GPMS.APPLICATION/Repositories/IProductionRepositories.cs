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
        Task<IEnumerable<Production>> GetProductionList();
        // Change DTO -1
        Task<ProductionDetailViewDTO> GetProductionDetail(int productionId);
        Task<Production> RequestProductionRevision(int productionId);
        Task<Production> DenyProduction(int productionId, int userId, string reason);
        Task<Production> UpdateProduction(int productionId, Production production);
        Task<IEnumerable<Production>> GetPendingProductionPlans();
        Task<IEnumerable<Production>> GetProductionPlanList();
        Task<Production> ConfigProductionPlan(int productionId, IEnumerable<ProductionPart> parts);
        Task<Production> GetProductionPlanDetail(int productionId);
        Task<Production> DenyProductionPlan(int productionId, int userId, string reason);


        //The new DTO
        Task<ProductionDetailViewDTO> GetProductionDetailView(int productionId);
        Task<IEnumerable<ProductionDetailViewDTO>> GetPendingProductionPlanViews();
        Task<IEnumerable<ProductionDetailViewDTO>> GetProductionListViews();
        Task<IEnumerable<ProductionDetailViewDTO>> GetProductionPlanViews();


    }
}
