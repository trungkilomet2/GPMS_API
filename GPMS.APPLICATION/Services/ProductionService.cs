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
        private readonly IBaseRepositories<Role> _roleRepositories;
        private readonly IBaseRepositories<User> _userRepositories;
        private readonly IBaseRepositories<Production> _prdRepo;
        private readonly IBaseRepositories<Order> _orderRepo;
        private readonly IBaseRepositories<ProductionRejectReason> _productionRejectRepo;
        private readonly IBaseRepositories<ProductionIssueLog> _productionIssueRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductionService(
            IBaseRepositories<Production> productionRepo, 
            IUnitOfWork unitOfWork, 
            IBaseRepositories<Role> roleRepositories, 
            IBaseRepositories<User> userRepositories, 
            IBaseRepositories<Order> orderRepo,
            IBaseRepositories<ProductionRejectReason> productionRejectRepo,
            IBaseRepositories<ProductionIssueLog> productionIssueRepo
            ) {
            _unitOfWork = unitOfWork;
            _prdRepo = productionRepo;
            _roleRepositories = roleRepositories;
            _userRepositories = userRepositories;
            _orderRepo = orderRepo;
            _productionRejectRepo = productionRejectRepo;
            _productionIssueRepo = productionIssueRepo;
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
                throw new ValidationException("Chỉ Owner và PM mới được quản lý Production");
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


        // OK -1
        public async Task<ProductionDetailViewDTO> GetProductionDetail(int productionId)
        {
            var production_detail = await _prdRepo.GetById(productionId);

            if (production_detail is null)
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
    //        production.StatusId = await _productionRepo.GetStatusIdByName(ProductionStatus_Constants.NeedUpdate);
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


        public async Task<Production> UpdatePMProduction(int production_id, int new_pm_id)
        {
            var existing_production = await _prdRepo.GetById(production_id);
            if (existing_production is null)
            {
                throw new ValidationException("Không tồn tại Production trong hệ thống");
            }
            if (existing_production.PmId == new_pm_id)
            {
                throw new ValidationException("Đơn hàng nay đang giao cho người này quản lý");
            }
            var check_user_system = await _userRepositories.GetById(new_pm_id);
            if (check_user_system is null)
            {
                throw new ValidationException("Người dùng không tồn tại trong hệ thống");
            }
            if (!check_user_system.Roles.Any(r => r.Name == Roles_Constants.PM || r.Name == Roles_Constants.Owner))
            {
                throw new ValidationException("Người dùng không đủ quyền để quản lý production này");
            }
            existing_production.PmId = new_pm_id;
            return await _prdRepo.Update(existing_production);
        }

        // New Coding for DTOs
        public async Task<ProductionDetailViewDTO> GetProductionDetailView(int productionId)
        {
            if(productionId <= 0)
            {
                throw new ValidationException("Production ID truyền vào phải là một số > 0");
            }
            var production = await GetProductionDetail(productionId);
            return production;
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



        public async Task<Production> ApproveProduction(int productionId, int actionByUserId)
        {
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            var actor = await _userRepositories.GetById(actionByUserId) ?? throw new ValidationException("Người thao tác không tồn tại");
            production.StatusId = ProductionStatus_Constants.Approval_ID;
            return await _prdRepo.Update(production);
        }

        public async Task<Production> RejectProduction(int productionId, int actionByUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ValidationException("Lý do từ chối là bắt buộc");
            }

            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            var actor = await _userRepositories.GetById(actionByUserId) ?? throw new ValidationException("Người thao tác không tồn tại");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                production.StatusId = ProductionStatus_Constants.Reject_ID;
                await _prdRepo.Update(production);
                await _productionRejectRepo.Create(new ProductionRejectReason
                {
                    ProductionId = productionId,
                    UserId = actionByUserId,
                    Reason = reason.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            });

            return await _prdRepo.GetById(productionId);
        }

        public async Task<IEnumerable<ProductionIssueLog>> GetProductionIssues(int productionId)
        {
            _ = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            return await _productionIssueRepo.GetAll(productionId);
        }

        public async Task<IEnumerable<ProductionIssueLog>> GetProductionIssueSummaryByType(int productionId)
        {
            var issues = (await GetProductionIssues(productionId)).ToList();
            return issues.GroupBy(x => x.TypeIssue)
                .Select(g => new ProductionIssueLog
                {
                    TypeIssue = g.Key,
                    CreatedAt = g.Max(x => x.CreatedAt),
                    Id = g.Count()
                }).ToList();
        }

        public async Task<ProductionIssueLog> CreateProductionIssue(ProductionIssueLog issue)
        {
            _ = await _prdRepo.GetById(issue.ProductionId) ?? throw new ValidationException("Production không tồn tại");
            _ = await _userRepositories.GetById(issue.CreatedBy) ?? throw new ValidationException("Người báo lỗi không tồn tại");
            issue.CreatedAt = DateTime.UtcNow;
            return await _productionIssueRepo.Create(issue);
        }

    }
}
