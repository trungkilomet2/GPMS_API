using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseProductionRepositories
    {
        Task<Production> CreateProduction(Production production);
        Task<IEnumerable<Production>> GetProductionList();
        Task<Production?> GetProductionDetail(int productionId);
        Task<Production> UpdateProduction(Production production);
        Task<int> GetStatusIdByName(string statusName);
        Task SaveRejectReason(int productionId, int userId, string reason);
        Task<IEnumerable<Production>> GetPendingProductionPlans();
        Task<IEnumerable<Production>> GetProductionPlanList();
        Task ReplaceProductionParts(int productionId, IEnumerable<ProductionPart> parts);
    }
}
