using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;

namespace GPMS.APPLICATION.Services
{
    public class ProductionPartService : IProductionPartRepositories
    {
        private readonly IBaseRepositories<ProductionPart> _partRepo;
        private readonly IBaseProductionPartAssignRepositories _partAssignRepo;
        private readonly IBaseRepositories<Production> _productionRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseWorkerRepository _workerSkill;
        private readonly IBaseRepositories<LeaveRequest> _leaveRequestRepo;
        private readonly IBaseRepositories<ProductionPartWorkLog> _workLogRepo;
        private readonly IBaseRepositories<Order> _orderRepo;

        private readonly IUnitOfWork _unitOfWork;

        public ProductionPartService(
            IBaseRepositories<ProductionPart> partRepo,
            IBaseRepositories<Production> productionRepo,
            IBaseRepositories<User> userRepo,
            IUnitOfWork unitOfWork,
            IBaseProductionPartAssignRepositories partAssignRepo,
            IBaseWorkerRepository workerSkill,
            IBaseRepositories<LeaveRequest> leaveRequestRepo,
            IBaseRepositories<ProductionPartWorkLog> workLogRepo,
            IBaseRepositories<Order> orderRepo
            )
        {
            _partRepo = partRepo;
            _productionRepo = productionRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _partAssignRepo = partAssignRepo;
            _workerSkill = workerSkill;
            _leaveRequestRepo = leaveRequestRepo;
            _workLogRepo = workLogRepo;
            _orderRepo = orderRepo;
        }

        public async Task<IEnumerable<ProductionPartDetailViewDTO>> GetPartsByProductionId(int productionId)
        {
            await EnsureProductionExists(productionId);
            var list_parts_in_productions = await _partRepo.GetAll(productionId);
            return await BuildViews(list_parts_in_productions);
        }

        public async Task<ProductionPartDetailViewDTO> GetPartAssignmentDetail(int partId)
        {
            if (partId <= 0)
            {
                throw new ValidationException("Part id phải > 0");
            }

            var part = await _partRepo.GetById(partId);

            if (part is null)
            {
                throw new ValidationException("Production part không tồn tại trong hệ thống");
            }

            return (await BuildViews(new[] { part })).First();
        }

        public async Task<IEnumerable<ProductionPartDetailViewDTO>> CreateParts(int productionId, IEnumerable<ProductionPart> parts)
        {
            await EnsureProductionExists(productionId);

            var validatedParts = (parts ?? Enumerable.Empty<ProductionPart>()).ToList();

            //if (!validatedParts.Any())
            //{
            //    throw new ValidationException("Danh sách production part không được rỗng");
            //}

            foreach (var part in validatedParts)
            {
                await ValidatePartInput(productionId, part);
            }
            List<ProductionPart> check_parts = new List<ProductionPart>();
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var part in validatedParts)
                {
                    check_parts.Add(await _partRepo.Create(part));
                }

                // Chuyển trạng thái thành Chờ Xét Duyệt Của Chủ Xưởng
                var production = await _productionRepo.GetById(productionId);
                if (production is not null)
                {
                    // Chuyển đổi trạng thái thành Chờ Xét Duyệt Kế Hoạch
                    production.StatusId = ProductionStatus_Constants.PendingPlan_ID;
                    await _productionRepo.Update(production);
                }

            });
            return await BuildViews(check_parts);
        }

        public async Task<ProductionPartDetailViewDTO> UpdatePart(int partId, ProductionPart part)
        {
            if (partId <= 0)
            {
                throw new ValidationException("Part id phải > 0");
            }

            var existing = await _partRepo.GetById(partId);
            if (existing is null)
            {
                throw new ValidationException("Production part không tồn tại trong hệ thống");
            }

            var productionId = part.ProductionId <= 0 ? existing.ProductionId : part.ProductionId;
            await ValidatePartInput(productionId, part);

            existing.PartName = part.PartName;
            existing.Cpu = part.Cpu;
            existing.StatusId = part.StatusId;
            existing.StartDate = part.StartDate;
            existing.EndDate = part.EndDate;

            var updated = await _partRepo.Update(existing);
            return (await BuildViews(new[] { updated })).First();
        }


        public async Task<ProductionPartDetailViewDTO> AssignWorkers(int partId, IEnumerable<int> workerIds)
        {
            if (partId <= 0)
            {
                throw new ValidationException("Part id phải > 0");
            }

            var workers = (workerIds ?? Enumerable.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (!workers.Any())
            {
                throw new ValidationException("Danh sách worker id không hợp lệ");
            }

            foreach (var workerId in workers)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is null)
                {
                    throw new ValidationException($"Worker id '{workerId}' không tồn tại");
                }
            }
            //     var updated = new ProductionPartDetailViewDTO();
            var updated = await _partAssignRepo.AssignWorkers(partId, workers);
            //   return null;
            return (await BuildViews(new[] { updated })).First();
        }

        public async Task DeletePart(int partId)
        {
            if (partId <= 0)
            {
                throw new ValidationException("Part id phải > 0");
            }

            var existing = await _partRepo.GetById(partId);
            if (existing is null)
            {
                throw new ValidationException("Production part không tồn tại trong hệ thống");
            }

            await _partRepo.Delete(partId);
        }

        private async Task EnsureProductionExists(int productionId)
        {
            if (productionId <= 0)
            {
                throw new ValidationException("Production id phải > 0");
            }

            var production = await _productionRepo.GetById(productionId);
            if (production is null)
            {
                throw new ValidationException("Production không tồn tại trong hệ thống");
            }
        }

        private async Task ValidatePartInput(int productionId, ProductionPart part)
        {
            await EnsureProductionExists(productionId);

            if (part is null)
            {
                throw new ValidationException("Yêu cầu thông tin công đoạn");
            }

            if (string.IsNullOrWhiteSpace(part.PartName))
            {
                throw new ValidationException("Tên công đoạn không được để trống");
            }


            if (part.Cpu <= 0)
            {
                throw new ValidationException("Đơn giá bán thành phẩm phải > 0");
            }

            if (part.StatusId <= 0)
            {
                throw new ValidationException("Trạng thái phải > 0");
            }

            if (part.StartDate.HasValue && part.EndDate.HasValue && part.EndDate < part.StartDate)
            {
                throw new ValidationException("Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu");
            }
        }

        // Hàm tổng quát dùng để trả về một Production Part Detail
        private async Task<IEnumerable<ProductionPartDetailViewDTO>> BuildViews(IEnumerable<ProductionPart> parts)
        {
            var result = new List<ProductionPartDetailViewDTO>();

            foreach (
                var part in parts)
            {
                // Lấy thông tin chi tiết của Part, bao gồm cả Team Leader và Assignees
                var detail = await _partRepo.GetById(part.Id);
                // Lấy danh sách người được giao việc, loại bỏ trùng lặp nếu có
                var assignees = detail.AssigneeIds.Distinct().ToList();
                // Lấy danh sách người dùng được giao
                var assigneeUsers = new List<User>();
                foreach (var assignee in assignees)
                {
                    // Lấy thống tin của người dùng
                    var user = await _userRepo.GetById(assignee);
                    if (user is not null)
                    {
                        assigneeUsers.Add(user);
                    }
                }
                // Thêm Người dùng vào kết quả trả về => Trả về một view gồm 3 khóa ngoại: Part, Team Leader, Assignees
                result.Add(new ProductionPartDetailViewDTO
                {
                    Part = detail,
                    Assignees = assigneeUsers
                });
            }

            return result;
        }

        public async Task<ProductionPartDetailViewDTO> RemoveWorker(int partId, int workerId)
        {
            if (partId <= 0 || workerId <= 0)
            {
                throw new ValidationException("Part id và worker id phải > 0");
            }
            var updated = await _partAssignRepo.RemoveWorker(partId, workerId);
            if (updated is null)
            {
                throw new ValidationException("Production part không tồn tại trong hệ thống");
            }
            return (await BuildViews(new[] { updated })).First();
        }



        public async Task<IEnumerable<AssignWorkerViewDTO>> ListAssignWorker(int pm_id, DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                throw new ValidationException("From date phải nhỏ hơn To date");
            }
            var listUser = await _partAssignRepo.ListWorkerWithPM(pm_id);
            List<AssignWorkerViewDTO> listAssignWorker = new List<AssignWorkerViewDTO>();
            foreach (var user in listUser)
            {
                FromDateToEndDate from_to_end = new FromDateToEndDate { UserId = user.Id, FromDate = fromDate, EndDate = toDate };
                var list_skill = await _workerSkill.GetWorkerSkillByUserId(user.Id);
                var list_leave_request_by_user = await _leaveRequestRepo.GetAll(from_to_end);
                listAssignWorker.Add(new AssignWorkerViewDTO
                {
                    Workers = user,
                    Skill_Of_Worker = list_skill,
                    LeaveRequest = list_leave_request_by_user
                });
            }
            return listAssignWorker;
        }


        public async Task<IEnumerable<ProductionPartWorkLog>> GetWorkLogs(int partId)
        {
            _ = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại");
            var logs = await _workLogRepo.GetAll(partId);
            var now = VietnamTime.Now();
            var normalized = new List<ProductionPartWorkLog>();
            foreach (var log in logs)
            {
                log.IsReadOnly = log.IsReadOnly || now - log.WorkDate > TimeSpan.FromHours(24);
                normalized.Add(log);
            }
            return normalized;
        }

        public async Task<ProductionPartWorkLog> CreateWorkLog(int partId, int userId, int quantity)
        {
            var productionPart = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại");

            if (!productionPart.AssigneeIds.Contains(userId))
            {
                throw new ValidationException("Nhân viên không được phân công cho làm công đoạn này");
            }

            var user = await _userRepo.GetById(userId) ?? throw new ValidationException("Worker không tồn tại");

            if (quantity <= 0) throw new ValidationException("Số lượng phải > 0");

            ProductionPartWorkLog returnData = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                //Lấy thông tin production part hiện tại
                var part = await _partRepo.GetById(partId);
                // Nếu production part đang ở trạng thái là chưa thực hiện hoặc đang sản xuất thì mới có thể cập nhật số lượng được
                if (part.StatusId > ProductionPart_Constrants.OnGoing_ID)
                {
                    throw new ValidationException("Không thể cập nhật công đoạn trong trạng thái này");
                }
                // lấy thông tin production
                var production = await _productionRepo.GetById(part.ProductionId);
                // lấy thông tin đơn hàng
                var getOrder = await _orderRepo.GetById(production.OrderId);
                // Nếu như số lượng ở trong đơn hàng vượt quá số lượng đơn hàng giao cho => Không thể cập nhật số lượng
                if (quantity > getOrder.Quantity)
                {
                    throw new ValidationException("Số lượng làm không thể lớn hơn số lượng đơn hàng giao cho");
                }
                // Lấy lịch sử submit sản lượng ở trong production part đấy
                var allWorkLogsInAPart = await _workLogRepo.GetAll(partId);

                int historyQuantitySubmits = 0;
                foreach (var logpart in allWorkLogsInAPart)
                {
                    historyQuantitySubmits += logpart.Quantity;
                }
                int nowQuantitySubmit = historyQuantitySubmits + quantity;
                // Nếu như đơn hàng chưa hoàn thành thì cập nhật trạng thái thành đang sản xuất
                if (nowQuantitySubmit > 0 && nowQuantitySubmit < getOrder.Quantity)
                {
                    productionPart.StatusId = ProductionPart_Constrants.OnGoing_ID;
                }
                if (nowQuantitySubmit > getOrder.Quantity)
                {
                    throw new ValidationException("Tổng số lượng làm không thể lớn hơn số lượng đơn hàng giao cho");
                }
                if (nowQuantitySubmit == getOrder.Quantity)
                {
                    productionPart.StatusId = ProductionPart_Constrants.Reviewing_ID;
                }
                // Update số lượng sản phẩm đã làm vào production part đó
                await _partRepo.Update(productionPart);

                returnData = await _workLogRepo.Create(new ProductionPartWorkLog
                {
                    PartId = partId,
                    UserId = userId,
                    Quantity = quantity,
                    WorkDate = VietnamTime.Now(),
                    IsReadOnly = false,
                    IsPayment = false
                });
            });

            return returnData;
        }

        public async Task<ProductionPartWorkLog> UpdateWorkLog(int partId, int workLogId, int quantity)
        {
            if (quantity <= 0) throw new ValidationException("Số lượng phải > 0");
            // Workflow: 
            /*
             1: Lấy thông tin tất cả các work log của công đoạn đó gồm : 
                - Thông tin worklog của tất cả các lần submit sản lượng của công đoạn đó của các nhân viên khác và của chính mình khác với ngày hôm nay
                - Lấy thông tin worklog của các nhân viên trong ngày hôm nay ngoại trừ bản thân mình
             2: Sau khi lấy được thông tin work log của toàn bộ nhân viên trong hệ thông và của chính mình 
                Cập nhật sản lượng worklog được nghiệm thu trong ngày nêu như : 
                - Sản lượng hôm nay < order.quantity thì trạng thái vẫn dữ nguyên : "Đang Sản Xuất"
                - Sản lượng hôm này = order. quantity thì trạng thái được cập nhật thành "Đang Xét Duyệt"
                - Sản lượng hôm nay > order.quantity thì throw lỗi không thể cập nhật sản lượng lớn hơn số lượng đơn hàng giao cho  
             */
            ProductionPartWorkLog returnData = null;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {   
                // Lấy thông tin production part hiện tại
                var part = await _partRepo.GetById(partId);
                // lấy thông tin production
                var production = await _productionRepo.GetById(part.ProductionId);
                // lấy thông tin đơn hàng
                var getOrder = await _orderRepo.GetById(production.OrderId);
                // Lấy thông tin tất cả các worklog của công đoạn đó bao gồm cả trong lịch sử:
                var historyWorkLogs = await _workLogRepo.GetAll(partId);
                // Tạo data sản lượng đầu ra của các worklog được submit
                int quantityHistoryWorkLog = 0;
                // Lấy thông tin ghi chép của công nhân hiện tại
                var log = await _workLogRepo.GetById(workLogId) ?? throw new ValidationException("Work log không tồn tại");
                if (log.PartId != partId) throw new ValidationException("Work log không thuộc công đoạn này");
                foreach (var worklog in historyWorkLogs)
                {
                    if(worklog.Id != workLogId)
                    {
                        quantityHistoryWorkLog += worklog.Quantity;
                    }
                }
                int nowQuantitySubmit = quantityHistoryWorkLog + quantity;

                if(nowQuantitySubmit < getOrder.Quantity)
                {
                    part.StatusId = ProductionPart_Constrants.OnGoing_ID;
                    log.Quantity = quantity;
                }

                if(nowQuantitySubmit == getOrder.Quantity)
                {
                    part.StatusId = ProductionPart_Constrants.Reviewing_ID;
                    log.Quantity = quantity;
                }

                if (nowQuantitySubmit > getOrder.Quantity)
                {
                    throw new ValidationException("Không thể cập nhật số lượng vượt quá số lượng đơn hàng đặt ra.");
                }
                returnData = await _workLogRepo.Update(log);
            });

            //var elapsed = VietnamTime.Now() - log.WorkDate;
            //if (log.IsReadOnly || elapsed > TimeSpan.FromHours(24))
            //{
            //    throw new ValidationException("Work log đã quá 24h, chỉ được xem");
            //}

            return returnData;
        }




    }
}