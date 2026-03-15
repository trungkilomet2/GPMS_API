using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using System.ComponentModel.DataAnnotations;

namespace GPMS.APPLICATION.Services
{
    public class ProductionService : IProductionRepositories
    {
        private readonly IBaseProductionRepositories _productionRepo;
        private readonly IBaseRepositories<Role> _roleRepositories;
        private readonly IBaseRepositories<User> _userRepositories;
        private readonly IBaseRepositories<Production> _prdRepo;
        private readonly IBaseRepositories<Order> _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductionService(IBaseRepositories<Production> productionRepo, IUnitOfWork unitOfWork, IBaseProductionRepositories prdRepo,
            IBaseRepositories<Role> roleRepositories, IBaseRepositories<User> userRepositories, IBaseRepositories<Order> orderRepo)
        {
            _productionRepo = prdRepo;
            _unitOfWork = unitOfWork;
            _prdRepo = productionRepo;
            _roleRepositories = roleRepositories;
            _userRepositories = userRepositories;
            _orderRepo = orderRepo;
        }

        public async Task<Production> CreateProduction(Production production)
        {
            if (production is null)
            {
                throw new ValidationException("Production data is required.");
            }
            // Kiểm tra người dùng trong hệ thống
            User check_user_system = await _userRepositories.GetById(production.PmId);
            if (check_user_system is null)
            {
                throw new ValidationException("Người dùng không tồn tại trong hệ thống");
            }
            IEnumerable<Role> roles_of_user = await _roleRepositories.GetAll(production.PmId);
            if (roles_of_user.Count() == 0)
            {
                throw new Exception("Người dùng không tồn tại role nào");
            }
            if (roles_of_user.Where(r => r.Name.Equals(Roles_Constants.Owner)).Count() == 0)
            {
                throw new ValidationException("Chỉ Owner mới được tạo Production");
            }

            // Kiêm tra đơn hàng trong hệ thống
            Order check_order_system = await _orderRepo.GetById(production.OrderId);
            if (check_order_system is null)
            {
                throw new ValidationException("Đơn hàng không tồn tại trong hệ thống");
            }
            if (check_order_system.Status != OrderStatus_Constants.Approved_ID)
            {
                throw new ValidationException("Đơn hàng không thể tạo kế hoạch sản xuất");
            }

            //    User isPmExist = _roleRepositories.GetById(production.PmId);
            return await _prdRepo.Create(production);
        }


        public async Task<IEnumerable<Production>> GetProductionList() => await _productionRepo.GetProductionList();

        public async Task<Production> GetProductionDetail(int productionId)
            => await _productionRepo.GetProductionDetail(productionId) ?? throw new Exception($"Production with id '{productionId}' not found.");

        // Yêu cầu chỉnh sửa kế hoạch cho sản xuất cho hợp lý
        public async Task<Production> RequestProductionRevision(int productionId)
        {
            var production = await GetProductionDetail(productionId);
            // Chueyenr đổi trạng thái thành yêu cầu chỉnh sửa
            production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.NeedUpdate);
            return await _productionRepo.UpdateProduction(production);
        }

        // Từ chối kế hoạch sản xuất với lý do cụ thể, lưu lại lịch sử từ chối
        public async Task<Production> DenyProduction(int productionId, int userId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new Exception("Deny reason is required.");

            var production = await GetProductionDetail(productionId);
            production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.Reject);
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
