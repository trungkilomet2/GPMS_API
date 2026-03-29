using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
        private readonly IBaseOrderRepositories _orderStatusRepo;
        private readonly IBaseWorkerRepository _workerRepo;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IBaseRepositories<ProductionPart> _productionPartRepo;
        private readonly IBaseRepositories<ProductionPartWorkLog> _productionPartWorkLogRepo;
        private readonly IBaseRepositories<CuttingNotebook> _cuttingNotebookRepo;
        private readonly IBaseRepositories<CuttingNotebookLog> _cuttingNotebookLogRepo;

        public ProductionService(
            IBaseRepositories<Production> productionRepo,
            IUnitOfWork unitOfWork,
            IBaseRepositories<Role> roleRepositories,
            IBaseRepositories<User> userRepositories,
            IBaseRepositories<Order> orderRepo,
            IBaseRepositories<ProductionRejectReason> productionRejectRepo,
            IBaseRepositories<ProductionIssueLog> productionIssueRepo,
            IBaseOrderRepositories orderStatusRepo,
            IBaseRepositories<ProductionPart> productionPartRepo,
            IBaseRepositories<ProductionPartWorkLog> productionPartWorkLogRepo,
            IBaseRepositories<CuttingNotebook> cuttingNotebookRepo,
            IBaseRepositories<CuttingNotebookLog> cuttingNotebookLogRepo,
            IBaseWorkerRepository workerRepo
            )
        {
            _unitOfWork = unitOfWork;
            _prdRepo = productionRepo;
            _roleRepositories = roleRepositories;
            _userRepositories = userRepositories;
            _orderRepo = orderRepo;
            _productionRejectRepo = productionRejectRepo;
            _productionIssueRepo = productionIssueRepo;
            _orderStatusRepo = orderStatusRepo;
            _productionPartRepo = productionPartRepo;
            _productionPartWorkLogRepo = productionPartWorkLogRepo;
            _cuttingNotebookRepo = cuttingNotebookRepo;
            _cuttingNotebookLogRepo = cuttingNotebookLogRepo;
            _workerRepo = workerRepo;
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
            if (roles_of_user.Where(r => r.Name.Equals(Roles_Constants.Owner) || r.Name.Equals(Roles_Constants.PM)).Count() == 0)
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
            if (_prdRepo.GetAll(check_order_system).Result.Count() > 0)
            {
                throw new ValidationException("Đơn hàng đang trong kế hoạch sản xuất hoặc đã hoàn thành rồi");
            }
            // TrungNT 30-03-2026: Nếu là Owner thì tự động duyệt kế hoạch sản xuất luôn, không cần phải qua bước chờ xét duyệt nữa.
            if (roles_of_user.Any(r => r.Name.Equals(Roles_Constants.Owner)))
            {
                production.StatusId = ProductionStatus_Constants.Approval_ID;
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
            if (productionId <= 0)
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
                    Order = order,
                    ProductionStatusName = GetStatusProductName(production.StatusId)
                });
            }
            return result;
        }

        // Chấp Nhận Yêu Cầu Sản Xuất Đến Từ Chủ Xưởng  
        public async Task<Production> ApproveProduction(int productionId)
        {
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            if (production.StatusId == ProductionStatus_Constants.Reject_ID)
                throw new ValidationException("Production này đã bị từ chối rồi");

            if (production.StatusId == ProductionStatus_Constants.Pending_ID)
            {
                production.StatusId = ProductionStatus_Constants.Approval_ID;
                return await _prdRepo.Update(production);
            }
            else
            {
                throw new ValidationException("Production chỉ Approve được khi đang ở trạng thái Chờ Xét Duyệt");
            }
        }

        public async Task<Production> RejectProduction(int productionId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ValidationException("Lý do từ chối là bắt buộc");
            }
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");

            switch (production.StatusId)
            {
                case ProductionStatus_Constants.Approval_ID:
                    throw new ValidationException("Production này đã được chấp nhận .");
                case ProductionStatus_Constants.Producting_ID:
                    throw new ValidationException("Production này đang được sản xuất .");
                case ProductionStatus_Constants.Reject_ID:
                    throw new ValidationException("Production này đã từ chối trước đó .");
                case ProductionStatus_Constants.Done_ID:
                    throw new ValidationException("Production này đã hoàn thành rồi .");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                production.StatusId = ProductionStatus_Constants.Reject_ID;
                await _prdRepo.Update(production);
                await _productionRejectRepo.Create(new ProductionRejectReason
                {
                    ProductionId = productionId,
                    UserId = production.PmId,
                    Reason = reason.Trim(),
                    CreatedAt = VietnamTime.Now()
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
            issue.CreatedAt = VietnamTime.Now();
            return await _productionIssueRepo.Create(issue);
        }

        public string GetStatusProductName(int statusId)
        {
            switch (statusId)
            {
                case ProductionStatus_Constants.Pending_ID:
                    return ProductionStatus_Constants.Pending;

                case ProductionStatus_Constants.Reject_ID:
                    return ProductionStatus_Constants.Reject;

                case ProductionStatus_Constants.Approval_ID:
                    return ProductionStatus_Constants.Approval;

                case ProductionStatus_Constants.PendingPlan_ID:
                    return ProductionStatus_Constants.PendingPlan;

                case ProductionStatus_Constants.NeedUpdatePlan_ID:
                    return ProductionStatus_Constants.NeedUpdatePlan;

                case ProductionStatus_Constants.Producting_ID:
                    return ProductionStatus_Constants.Producting;

                case ProductionStatus_Constants.Done_ID:
                    return ProductionStatus_Constants.Done;

                default:
                    return "Unknown Status";
            }
        }

        public async Task<Production> ApproveProductionPlan(int productionId)
        {

            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");

            if (production.StatusId == ProductionStatus_Constants.Reject_ID)
                throw new ValidationException("Production này đã bị từ chối rồi");


            if (production.StatusId == ProductionStatus_Constants.PendingPlan_ID)
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
               {
                   // Chấp Nhận Production Plan Và Chuyển Về Trạng Thái Đang Sản Xuất và thông báo email đến PM
                   production.StatusId = ProductionStatus_Constants.Producting_ID;
                   // Cập Nhật ngày hoàn thành Production
                   production.StartDate = DateOnly.FromDateTime(VietnamTime.Now());
                   // Cập Nhật Trạng Thái Order => Đang Sản Xuất Và thông báo đến email của người đặt hàng
                   await _prdRepo.Update(production);
                   // Update Order Status
                   var order = await _orderRepo.GetById(production.OrderId);
                   if (order is null)
                   {
                       throw new Exception("Không tồn tại đơn hàng trong hệ thống");
                   }
                   // Chuyển đơn hàng thành trạng thái đang sản xuất và thông báo đến Customer
                   await _orderStatusRepo.ChangeStatus(production.OrderId, OrderStatus_Constants.Producting_ID);
               });
                return await _prdRepo.GetById(productionId);
            }
            else
            {
                throw new ValidationException("Production Plan chỉ chấp nhận được khi đang ở trạng thái Chờ Xét Duyệt Kế Hoạch");
            }
        }

        public async Task<Production> NeedUpdateProductionPlan(int productionId)
        {
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            if (production.StatusId == ProductionStatus_Constants.Reject_ID)
                throw new ValidationException("Production này đã bị từ chối rồi");

            if (production.StatusId == ProductionStatus_Constants.PendingPlan_ID)
            {
                production.StatusId = ProductionStatus_Constants.NeedUpdatePlan_ID;
                return await _prdRepo.Update(production);
            }
            else
            {
                throw new ValidationException("Production chỉ Cần Cập Nhật được khi đang ở trạng thái Chờ Xét Duyệt Kế Hoạch");
            }
        }

        public async Task<ProductionRejectReason> ProductionRejectReasonDetail(int productionId)
        {
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");

            var production_reject_reason_detail = await _productionRejectRepo.GetById(productionId);

            if (production_reject_reason_detail is null)
            {
                throw new ValidationException("Không tổn tại lý do từ chối.");
            }

            return production_reject_reason_detail;
        }


        //========================================================================================
        // 26-03-2026: Nhóm API phân tích sản lượng
        //========================================================================================
        public async Task<IEnumerable<ProductionWorkerOutputViewDTO>> GetProductionWorkerOutput(int productionId)
        {
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            // Lấy danh sách Worker làm việc trong Production đấy => Lấy từ PM được assign
            WorkerByManagerDTO managerData = new WorkerByManagerDTO() { ManagerId = production.PmId };

            var users = (await _workerRepo.GetAll(managerData)).ToList();
            var partIds = (await _productionPartRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();
            var notebookIds = (await _cuttingNotebookRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();
            var partLogs = (await _productionPartWorkLogRepo.GetAll(null)).Where(x => partIds.Contains(x.PartId)).ToList();
            var cuttingLogs = (await _cuttingNotebookLogRepo.GetAll(null)).Where(x => notebookIds.Contains(x.NotebookId)).ToList();
            var issueLogs = (await _productionIssueRepo.GetAll(productionId)).ToList();

            return users.Select(user => new ProductionWorkerOutputViewDTO
            {
                WorkerId = user.Id,
                WorkerName = user.FullName,
                ProductionId = productionId,
                CuttingOutput = cuttingLogs.Where(x => x.UserId == user.Id).Sum(x => x.ProductQty),
                SewingOutput = partLogs.Where(x => x.UserId == user.Id).Sum(x => x.Quantity),
                IssueCount = issueLogs.Count(x => x.CreatedBy == user.Id || x.AssignedTo == user.Id)
            }).ToList();
        }

        public async Task<IEnumerable<WorkerProductivityHistoryViewDTO>> GetAllWorkersProductivityHistory()
        {
            var users = (await _userRepositories.GetAll(null)).ToDictionary(x => x.Id, x => x.FullName);
            var parts = (await _productionPartRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.ProductionId);
            var notebooks = (await _cuttingNotebookRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.ProductionId);
            var result = new List<WorkerProductivityHistoryViewDTO>();

            foreach (var workLog in await _productionPartWorkLogRepo.GetAll(null))
            {
                if (!users.ContainsKey(workLog.UserId) || !parts.ContainsKey(workLog.PartId)) continue;
                result.Add(new WorkerProductivityHistoryViewDTO
                {
                    WorkerId = workLog.UserId,
                    WorkerName = users[workLog.UserId],
                    ProductionId = parts[workLog.PartId],
                    SourceType = "PartWorkLog",
                    SourceId = workLog.Id,
                    Quantity = workLog.Quantity,
                    SubmittedAt = workLog.WorkDate
                });
            }

            foreach (var cuttingLog in await _cuttingNotebookLogRepo.GetAll(null))
            {
                if (!users.ContainsKey(cuttingLog.UserId) || !notebooks.ContainsKey(cuttingLog.NotebookId)) continue;
                result.Add(new WorkerProductivityHistoryViewDTO
                {
                    WorkerId = cuttingLog.UserId,
                    WorkerName = users[cuttingLog.UserId],
                    ProductionId = notebooks[cuttingLog.NotebookId],
                    SourceType = "CuttingNotebookLog",
                    SourceId = cuttingLog.Id,
                    Quantity = cuttingLog.ProductQty,
                    SubmittedAt = cuttingLog.DateCreate?.ToDateTime(TimeOnly.MinValue),
                    Note = cuttingLog.Note
                });
            }

            return result.OrderByDescending(x => x.SubmittedAt).ToList();
        }

        public async Task<ProductionOutputSummaryViewDTO> GetProductionOutputSummary(int productionId)
        {
            _ = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");

            var partIds = (await _productionPartRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();
            var notebookIds = (await _cuttingNotebookRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();

            var partOutput = (await _productionPartWorkLogRepo.GetAll(null))
                .Where(x => partIds.Contains(x.PartId))
                .Sum(x => x.Quantity);

            var cuttingOutput = (await _cuttingNotebookLogRepo.GetAll(null))
                .Where(x => notebookIds.Contains(x.NotebookId))
                .Sum(x => x.ProductQty);

            var issueCount = (await _productionIssueRepo.GetAll(productionId)).Count();

            return new ProductionOutputSummaryViewDTO
            {
                ProductionId = productionId,
                TotalCuttingOutput = cuttingOutput,
                TotalSewingOutput = partOutput,
                TotalIssueCount = issueCount
            };
        }

        public async Task<IEnumerable<WorkerProductivityHistoryViewDTO>> GetWorkerProductivityHistory(int workerId)
        {
            _ = await _userRepositories.GetById(workerId) ?? throw new ValidationException("Worker không tồn tại");
            return (await GetAllWorkersProductivityHistory()).Where(x => x.WorkerId == workerId);
        }

        public async Task<IEnumerable<WorkerAssignedPlanViewDTO>> GetWorkerAssignedPlans(int workerId)
        {
            _ = await _userRepositories.GetById(workerId) ?? throw new ValidationException("Worker không tồn tại");

            var workerParts = (await _productionPartRepo.GetAll(null))
                .Where(x => x.AssigneeIds.Contains(workerId))
                .ToList();

            var groupedByProduction = workerParts.GroupBy(x => x.ProductionId);
            var result = new List<WorkerAssignedPlanViewDTO>();

            foreach (var group in groupedByProduction)
            {
                var production = await _prdRepo.GetById(group.Key);
                if (production is null) continue;

                var order = await _orderRepo.GetById(production.OrderId);
                if (order is null) continue;

                result.Add(new WorkerAssignedPlanViewDTO
                {
                    ProductionId = production.Id,
                    OrderId = order.Id,
                    OrderName = order.OrderName,
                    StatusId = production.StatusId,
                    PartNames = group.Select(x => x.PartName).Distinct().ToList()
                });
            }

            return result;
        }

        public async Task<Production> CompleteProduction(int productionId)
        {   
            // Lấy thông tin production
            var production = await _prdRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            // Lấy thông tin production trở thành trạng thái đang sản xuất
            if (production.StatusId != ProductionStatus_Constants.Producting_ID)
            {
                throw new ValidationException("Chỉ có thể hoàn thành production đang ở trạng thái Đang Sản Xuất");
            }
            // Lấy thông tin tất cả các Production Part trong hệ thống
            var parts = (await _productionPartRepo.GetAll(productionId)).ToList();
            if (parts.Count == 0)
            {
                throw new ValidationException("Production chưa có công đoạn để hoàn thành");
            }

            if (parts.Any(x => x.StatusId != ProductionPart_Constrants.Done_ID))
            {
                throw new ValidationException("Chỉ có thể hoàn thành production khi tất cả công đoạn đã Hoàn Thành");
            }
            
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {   
                // Set trạng thái cho production thành hoàn thành
                production.StatusId = ProductionStatus_Constants.Done_ID;
                // Cập nhật thời gian kết thúc của một production
                production.EndDate = DateOnly.FromDateTime(VietnamTime.Now());
                // Cập nhật trạng thái của production
                await _prdRepo.Update(production);
                // Cập nhật trạng thái của Order thành hoàn thành
                await _orderStatusRepo.ChangeStatus(production.OrderId, OrderStatus_Constants.Done_ID);
            });
            // Trả về thông tin của production đấy
            return await _prdRepo.GetById(productionId) ?? throw new ValidationException("Không tìm thấy production sau khi cập nhật");
        }



    }
}
