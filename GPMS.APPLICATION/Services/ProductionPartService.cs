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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        private readonly IBaseRepositories<ProductionPartOrderSize> _partOrderSizeRepo;
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
            IBaseRepositories<Order> orderRepo,
            IBaseRepositories<ProductionPartOrderSize> partOrderSizeRepo
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
            _partOrderSizeRepo = partOrderSizeRepo;
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

                // TRUNGNT FIX: Khi PM cập nhật lại kế hoạch part bằng danh sách mới,
                // hệ thống cần đồng bộ theo danh sách mới để tránh part cũ còn tồn tại và ghi đè dữ liệu.
                var existingParts = (await _partRepo.GetAll(productionId)).ToList();
                foreach (var existingPart in existingParts)
                {
                    var existingLogs = await _workLogRepo.GetAll(existingPart.Id);
                    if (existingLogs.Any())
                    {
                        throw new ValidationException("Không thể thay mới danh sách công đoạn vì đã phát sinh log sản lượng ở công đoạn cũ");
                    }
                    await _partRepo.Delete(existingPart.Id);
                }

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

            foreach (var workerId in workers)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is null)
                {
                    throw new ValidationException($"Worker id '{workerId}' không tồn tại");
                }
            }

            var part = await _partRepo.GetById(partId);
            if (part is null)
            {
                throw new ValidationException("Production part không tồn tại trong hệ thống");
            }
            var production = await _productionRepo.GetById(part.ProductionId);
            if (production is null)
            {
                throw new ValidationException("Production không tồn tại trong hệ thống");
            }
            if (production.StatusId != ProductionStatus_Constants.Producting_ID)
            {
                throw new ValidationException("Chỉ có thể phân công công đoạn khi production đang ở trạng thái Đang Sản Xuất");
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

        }

        // Hàm tổng quát dùng để trả về một Production Part Detail
        private async Task<IEnumerable<ProductionPartDetailViewDTO>> BuildViews(IEnumerable<ProductionPart> parts)
        {
            var result = new List<ProductionPartDetailViewDTO>();

            foreach (
                var part in parts)
            {
                // Lấy thông tin chi tiết của Part Order Size, bao gồm cả
                var detail = await _partOrderSizeRepo.GetById(part.Id);
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
                    PartOrderSize = detail,
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
                // TRUNGNT FIX: Không cho phép ghi log work khi production đã hoàn thành
                // để tránh việc trạng thái part bị trả ngược về đang sản xuất.
                if (production.StatusId == ProductionStatus_Constants.Done_ID)
                {
                    throw new ValidationException("Production đã hoàn thành, không thể ghi nhận thêm sản lượng");
                }
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
            if (quantity < 0) throw new ValidationException("Số lượng phải >= 0");
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
                // TRUNGNT FIX: Không cho phép sửa log khi production đã hoàn thành
                // để tránh cập nhật lại trạng thái part sai nghiệp vụ.
                if (production.StatusId == ProductionStatus_Constants.Done_ID)
                {
                    throw new ValidationException("Production đã hoàn thành, không thể cập nhật log sản lượng");
                }
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
                    if (worklog.Id != workLogId)
                    {
                        quantityHistoryWorkLog += worklog.Quantity;
                    }
                }
                int nowQuantitySubmit = quantityHistoryWorkLog + quantity;
                if (nowQuantitySubmit < getOrder.Quantity)
                {
                    part.StatusId = ProductionPart_Constrants.OnGoing_ID;
                    log.Quantity = quantity;
                }
                if (nowQuantitySubmit == getOrder.Quantity)
                {
                    part.StatusId = ProductionPart_Constrants.Reviewing_ID;
                    log.Quantity = quantity;
                }
                if (nowQuantitySubmit > getOrder.Quantity)
                {
                    throw new ValidationException("Không thể cập nhật số lượng vượt quá số lượng đơn hàng đặt ra.");
                }
                await _partRepo.Update(part);
                returnData = await _workLogRepo.Update(log);
            });

            //var elapsed = VietnamTime.Now() - log.WorkDate;
            //if (log.IsReadOnly || elapsed > TimeSpan.FromHours(24))
            //{
            //    throw new ValidationException("Work log đã quá 24h, chỉ được xem");
            //}

            return returnData;
        }

        public async Task<ProductionPartDetailViewDTO> DoneAPart(int partId)
        {

            var part = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại trong hệ thống");
            // Kiểm tra part đấy đã về trạng thái chờ nghiệm thu chưa nếu như chưa thì không thể hoàn thành công đoạn đấy được
            if (part.StatusId != ProductionPart_Constrants.Reviewing_ID)
            {
                throw new ValidationException("Chỉ có thể hoàn thành công đoạn đang ở trạng thái Chờ Nghiệm Thu");
            }
            // Kiểm tra toàn bộ số lượng part log work của một part đấy nếu như nó chưa bằng sản lượng của production 
            // Thì không thể hoàn thành công đoạn đó được

            // Lấy thông tin production thông qua part đấy
            var production = await _productionRepo.GetById(part.ProductionId) ?? throw new ValidationException("Production không tồn tại trong hệ thống");
            // Lấy thông tin của Order trong hệ thống thông qua production đấy
            var order = await _orderRepo.GetById(production.OrderId) ?? throw new ValidationException("Order không tồn tại trong hệ thống");

            int totalQuantityWorkLog = 0;
            // Lấy tất cả các báo cáo thông qua part id đấy
            var allQuantityInAPart = await _workLogRepo.GetAll(partId);
            // Tổng hợp số lượng
            foreach(var worklog in allQuantityInAPart)
            {
                totalQuantityWorkLog += worklog.Quantity;
            }
            if(totalQuantityWorkLog != order.Quantity)
            {
                throw new ValidationException("Số lượng hoàn thành không khớp với số lượng trong Order");
            }
            // Cập nhật trạng thái của một Production Part
            part.StatusId = ProductionPart_Constrants.Done_ID;
            await _partRepo.Update(part);
            return (await BuildViews(new[] { part })).First();
        }

        public async Task<IEnumerable<User>> GetIssueWorkersByWorkLogs(int partId)
        {
            var part = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại trong hệ thống");
            var logs = await _workLogRepo.GetAll(part.Id);
            var workerIds = logs.Select(x => x.UserId).Distinct().ToList();

            var workers = new List<User>();
            foreach (var workerId in workerIds)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is not null)
                {
                    workers.Add(worker);
                }
            }
            return workers;
        }

        public async Task<PartPaymentCompletionViewDTO> CompletePartPayment(int partId, IEnumerable<int> workLogIds)
        {
            var part = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại trong hệ thống");
            var selectedLogIds = (workLogIds ?? Enumerable.Empty<int>()).Where(x => x > 0).Distinct().ToList();
            if (selectedLogIds.Count == 0)
            {
                throw new ValidationException("Danh sách work log cần trả lương không hợp lệ");
            }
            var now = VietnamTime.Now();
            var today = DateOnly.FromDateTime(now);
            var updatedCount = 0;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var logId in selectedLogIds)
                {
                    var log = await _workLogRepo.GetById(logId) ?? throw new ValidationException($"Không tồn tại work log ID = {logId}");
                    if (log.PartId != part.Id)
                    {
                        throw new ValidationException($"Work log ID = {logId} không thuộc công đoạn hiện tại");
                    }
                    if (DateOnly.FromDateTime(log.WorkDate) == today)
                    {
                        throw new ValidationException($"Work log ID = {logId} thuộc ngày hôm nay nên chưa thể thanh toán");
                    }
                    if (log.IsPayment)
                    {
                        continue;
                    }
                    log.IsPayment = true;
                    log.IsReadOnly = true;
                    await _workLogRepo.Update(log);
                    updatedCount++;
                }
            });

            return new PartPaymentCompletionViewDTO
            {
                PartId = partId,
                AffectedLogs = updatedCount,
                PaidAt = now
            };
        }


        public async Task<ProductionPartCompletionEstimateViewDTO> EstimatePartCompletion(int partId, IEnumerable<int> workerIds)
        {
            var part = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại trong hệ thống");
            var production = await _productionRepo.GetById(part.ProductionId) ?? throw new ValidationException("Production không tồn tại");
            var order = await _orderRepo.GetById(production.OrderId) ?? throw new ValidationException("Order không tồn tại");

            var selectedWorkers = (workerIds ?? Enumerable.Empty<int>()).Where(x => x > 0).Distinct().ToList();
            if (selectedWorkers.Count == 0)
            {
                throw new ValidationException("Danh sách worker dự kiến không hợp lệ");
            }

            var totalDone = (await _workLogRepo.GetAll(part.Id)).Sum(x => x.Quantity);
            var remaining = Math.Max(0, order.Quantity - totalDone);

            var allLogs = (await _workLogRepo.GetAll(null))
                .Where(x => selectedWorkers.Contains(x.UserId))
                .ToList();

            var dailyCapacity = allLogs.Count == 0
                ? selectedWorkers.Count * 10
                : Math.Max(1, (int)Math.Ceiling(allLogs
                    .GroupBy(x => new { x.UserId, Day = DateOnly.FromDateTime(x.WorkDate) })
                    .Select(g => g.Sum(x => x.Quantity))
                    .Average()));

            var estimatedDays = remaining == 0 ? 0 : (int)Math.Ceiling((decimal)remaining / dailyCapacity);

            return new ProductionPartCompletionEstimateViewDTO
            {
                PartId = part.Id,
                ProductionId = part.ProductionId,
                RemainingQuantity = remaining,
                EstimatedDailyCapacity = dailyCapacity,
                EstimatedDaysToComplete = estimatedDays,
                EstimatedFinishDate = DateOnly.FromDateTime(VietnamTime.Now().AddDays(estimatedDays))
            };
        }

        public async Task<IEnumerable<ProductionWorkerProgressChartViewDTO>> GetProductionWorkerProgressChart(int productionId)
        {
            var production = await _productionRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            var order = await _orderRepo.GetById(production.OrderId) ?? throw new ValidationException("Order không tồn tại");

            var partIds = (await _partRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();
            var logs = (await _workLogRepo.GetAll(null)).Where(x => partIds.Contains(x.PartId)).ToList();
            var workerIds = logs.Select(x => x.UserId).Distinct().ToList();

            var result = new List<ProductionWorkerProgressChartViewDTO>();
            foreach (var workerId in workerIds)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is null) continue;
                var totalOutput = logs.Where(x => x.UserId == workerId).Sum(x => x.Quantity);
                var percent = order.Quantity <= 0 ? 0 : Math.Round((decimal)totalOutput * 100 / order.Quantity, 2);
                result.Add(new ProductionWorkerProgressChartViewDTO
                {
                    WorkerId = workerId,
                    WorkerName = worker.FullName,
                    ProductionId = productionId,
                    TotalOutput = totalOutput,
                    ProgressPercent = percent
                });
            }
            return result.OrderByDescending(x => x.ProgressPercent);
        }

        public async Task<IEnumerable<WorkerProductivityScoreViewDTO>> GetWorkerProductivityScores(int productionId)
        {
            _ = await _productionRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            var partIds = (await _partRepo.GetAll(productionId)).Select(x => x.Id).ToHashSet();
            var logs = (await _workLogRepo.GetAll(null)).Where(x => partIds.Contains(x.PartId)).ToList();
            var workerIds = logs.Select(x => x.UserId).Distinct().ToList();

            var scores = new List<WorkerProductivityScoreViewDTO>();
            foreach (var workerId in workerIds)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is null) continue;

                var workerLogs = logs.Where(x => x.UserId == workerId).ToList();
                var totalOutput = workerLogs.Sum(x => x.Quantity);
                var activeDays = workerLogs.Select(x => DateOnly.FromDateTime(x.WorkDate)).Distinct().Count();
                var avgPerDay = activeDays == 0 ? 0 : (decimal)totalOutput / activeDays;
                var qualityPenalty = workerLogs.Count(x => x.IsReadOnly && !x.IsPayment) * 0.5m;
                var score = Math.Round(Math.Max(0, avgPerDay - qualityPenalty), 2);

                scores.Add(new WorkerProductivityScoreViewDTO
                {
                    WorkerId = workerId,
                    WorkerName = worker.FullName,
                    ProductionId = productionId,
                    TotalOutput = totalOutput,
                    IssueCount = workerLogs.Count(x => x.IsReadOnly && !x.IsPayment),
                    ProductivityScore = score
                });
            }

            return scores.OrderByDescending(x => x.ProductivityScore);
        }







    }
}