using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
                throw new ValidationException("Đơn hàng không thể tạo kế hoạch sản xuất - Không ở trạng thái Chấp Nhận");
            }
            var new_production = await _prdRepo.Create(production);
            return new_production is null ? throw new Exception("Tạo đơn hàng không thành công") : new_production;
        }


        public async Task<IEnumerable<Production>> GetProductionList()
        {
            var data = await _productionRepo.GetProductionList();
            return data;
        }
        // OK -1
        public async Task<ProductionDetailViewDTO> GetProductionDetail(int productionId)
        {
            var production_detail = await _prdRepo.GetById(productionId);
            
            if(production_detail is null)
            {
                throw new ValidationException($"Production with id '{productionId}' not found.");
            }
            var pm = await _userRepositories.GetById(production_detail.PmId);
            var order = await _orderRepo.GetById(production_detail.OrderId);
            return new ProductionDetailViewDTO
            {
                Production = production_detail,
                ProjectManager = pm,
                Order = order
            };
        }
        // Yêu cầu chỉnh sửa kế hoạch cho sản xuất cho hợp lý
        public async Task<Production> RequestProductionRevision(int productionId)
        {   
            // Lấy toàn bộ thông tin của production đấy 
            var production = await _prdRepo.GetById(productionId);
            // Chueyenr đổi trạng thái thành yêu cầu chỉnh sửa
            production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.NeedUpdate);
            return await _prdRepo.Update(production);
        }

        // Từ chối kế hoạch sản xuất với lý do cụ thể, lưu lại lịch sử từ chối

        ///---------------------------------------------- Đang Xem Sett -------------------------------------------
        
        //public async Task<Production> DenyProduction(int productionId, int userId, string reason)
        //{
        //    if (string.IsNullOrWhiteSpace(reason)) throw new Exception("Deny reason is required.");

        //    var production = await GetProductionDetail(productionId);
        //    production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.Reject);
        //    var updated = await _productionRepo.UpdateProduction(production);
        //    await _productionRepo.SaveRejectReason(productionId, userId, reason);
        //    return updated;
        //}

        ///----------------------------------------------/////////////////////////////-------------------------------------------


        public async Task<Production> UpdatePMProduction(int production_id,int new_pm_id)
        {
            var existing_production = await _prdRepo.GetById(production_id);

            if(existing_production is not null)
            {
                throw new ValidationException("Không tồn tại Production trong hệ thống");
            }
            existing_production.PmId = new_pm_id;
            return await _productionRepo.UpdateProduction(existing_production);
        }

        public async Task<IEnumerable<Production>> GetPendingProductionPlans()
        {
            var data = await GetProductionList();
            data.AsQueryable().Where(p => p.StatusId == ProductionStatus_Constants.Pending_ID);
            return data;
        } 

        public async Task<IEnumerable<Production>> GetProductionPlanList() => await _productionRepo.GetProductionPlanList();

        public async Task<Production> ConfigProductionPlan(int productionId, IEnumerable<ProductionPart> parts)
        {
            _ = await GetProductionDetail(productionId);
            await _productionRepo.ReplaceProductionParts(productionId, parts);
            throw new Exception("hmmmm");
           // return await GetProductionDetail(productionId);
        }

        public async Task<Production> GetProductionPlanDetail(int productionId) => throw new Exception("hmmmm");


        public async Task<Production> DenyProductionPlan(int productionId, int userId, string reason)
            =>             throw new Exception("hmmmm");

        // New Coding for DTOs
        public async Task<ProductionDetailViewDTO> GetProductionDetailView(int productionId)
        {
            var production = await GetProductionDetail(productionId);
            throw new Exception("hmmmm");

        }

        public async Task<IEnumerable<ProductionDetailViewDTO>> GetPendingProductionPlanViews()
        {
            var productions = await GetPendingProductionPlans();
            var result = new List<ProductionDetailViewDTO>();
            foreach (var production in productions)
            {
                var pm = await _userRepositories.GetById(production.PmId);
                var order = await _orderRepo.GetById(production.OrderId);
                result.Add(new ProductionDetailViewDTO
                {
                    Production = production,
                    ProjectManager = pm,
                    Order = order
                });
            }
            return result;
        }

        public async Task<IEnumerable<ProductionDetailViewDTO>> GetProductionPlanViews()
        {
            var productions = await GetProductionPlanList();
            var result = new List<ProductionDetailViewDTO>();
            foreach (var production in productions)
            {
                var pm = await _userRepositories.GetById(production.PmId);
                var order = await _orderRepo.GetById(production.OrderId);
                result.Add(new ProductionDetailViewDTO
                {
                    Production = production,
                    ProjectManager = pm,
                    Order = order
                });
            }
            return result;
        }

        //Ussing --------------------------------
        public async Task<IEnumerable<ProductionDetailViewDTO>> GetProductionListViews()
        {
            var productions = await _prdRepo.GetAll(null);
            var result = new List<ProductionDetailViewDTO>();
            foreach (var production in productions)
            {
                var pm = await _userRepositories.GetById(production.PmId);
                var order = await _orderRepo.GetById(production.OrderId);
                result.Add(new ProductionDetailViewDTO
                {
                    Production = production,
                    ProjectManager = pm,
                    Order = order
                });
            }
            return result;
        }
    }
}
