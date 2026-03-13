using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Services
{
    public class ProductionService : IProductionRepositories
    {
        private readonly IBaseProductionRepositories _productionRepo;

        public ProductionService(IBaseProductionRepositories productionRepo)
        {
            _productionRepo = productionRepo;
        }

        public async Task<Production> CreateProduction(Production production)
        {
            if (production is null) throw new Exception("Failed to create production.");
            return await _productionRepo.CreateProduction(production);
        }

        public async Task<IEnumerable<Production>> GetProductionList() => await _productionRepo.GetProductionList();

        public async Task<Production> GetProductionDetail(int productionId)
            => await _productionRepo.GetProductionDetail(productionId) ?? throw new Exception($"Production with id '{productionId}' not found.");

        public async Task<Production> RequestProductionRevision(int productionId)
        {
            var production = await GetProductionDetail(productionId);
            production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.RevisionRequested);
            return await _productionRepo.UpdateProduction(production);
        }

        public async Task<Production> DenyProduction(int productionId, int userId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new Exception("Deny reason is required.");

            var production = await GetProductionDetail(productionId);
            production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.Rejected);
            var updated = await _productionRepo.UpdateProduction(production);
            await _productionRepo.SaveRejectReason(productionId, userId, reason);
            return updated;
        }

        public async Task<Production> UpdateProduction(int productionId, Production production)
        {
            var existing = await GetProductionDetail(productionId);
            existing.PmId = production.PmId;
            existing.OrderId = production.OrderId;
            existing.StartDate = production.StartDate;
            existing.EndDate = production.EndDate;
            existing.StatusId = production.StatusId;
            return await _productionRepo.UpdateProduction(existing);
        }

        public async Task<IEnumerable<Production>> GetPendingProductionPlans() => await _productionRepo.GetPendingProductionPlans();

        public async Task<IEnumerable<Production>> GetProductionPlanList() => await _productionRepo.GetProductionPlanList();

        public async Task<Production> ConfigProductionPlan(int productionId, IEnumerable<ProductionPart> parts)
        {
            _ = await GetProductionDetail(productionId);
            await _productionRepo.ReplaceProductionParts(productionId, parts);
            return await GetProductionDetail(productionId);
        }

        public async Task<Production> GetProductionPlanDetail(int productionId) => await GetProductionDetail(productionId);

        public async Task<Production> DenyProductionPlan(int productionId, int userId, string reason)
            => await DenyProduction(productionId, userId, reason);
    }
}
