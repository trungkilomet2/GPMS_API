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
        private readonly IBaseRepositories<OrderSize> _orderSizeRepo;
        private readonly IBaseRepositories<Size> _sizeRepo;
        private readonly IBaseRepositories<ProductionPartOrderSize> _partOrderSizeRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepositories<ProductionIssueLog> _issueRepo;
        private readonly IBaseRepositories<Delivery> _deliveryRepo;

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
            IBaseRepositories<ProductionPartOrderSize> partOrderSizeRepo,
            IBaseRepositories<OrderSize> orderSizeRepo,
            IBaseRepositories<Size> sizeRepo,
            IBaseRepositories<ProductionIssueLog> issueRepo,
            IBaseRepositories<Delivery> deliveryRepo
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
            _orderSizeRepo = orderSizeRepo;
            _sizeRepo = sizeRepo;
            _issueRepo = issueRepo;
            _deliveryRepo = deliveryRepo;
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
            if (productionId <= 0)
            {
                throw new ValidationException("Production id phải > 0");
            }

            var production = await _productionRepo.GetById(productionId);
            if (production is null)
            {
                throw new ValidationException("Production không tồn tại trong hệ thống");
            }

            if (production.StatusId == ProductionStatus_Constants.Pending_ID ||
                production.StatusId == ProductionStatus_Constants.Reject_ID ||
                production.StatusId == ProductionStatus_Constants.PendingPlan_ID ||
                production.StatusId == ProductionStatus_Constants.Producting_ID ||
                production.StatusId == ProductionStatus_Constants.Done_ID
                )
            {
                throw new ValidationException("Production không được phép tạo kế hoạch");
            }

            var validatedParts = (parts ?? Enumerable.Empty<ProductionPart>()).ToList();

            foreach (var part in validatedParts)
            {
                await ValidatePartInput(productionId, part);
            }

            var listOrderSizeByCustomer = await _orderSizeRepo.GetAll(production.OrderId);

            List<ProductionPart> check_parts = new List<ProductionPart>();
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {

                // TRUNGNT FIX: Khi PM cập nhật lại kế hoạch part bằng danh sách mới,
                // hệ thống cần đồng bộ theo danh sách mới để tránh part cũ còn tồn tại và ghi đè dữ liệu.
                // Xóa luôn toàn bộ production part size cũ đi
                var existingParts = (await _partRepo.GetAll(productionId)).ToList();
                foreach (var existingPart in existingParts)
                {
                    var existingLogs = await _workLogRepo.GetAll(existingPart.Id);
                    if (existingLogs.Any())
                    {
                        throw new ValidationException("Không thể thay mới danh sách công đoạn vì đã phát sinh log sản lượng ở công đoạn cũ");
                    }
                    // Xóa luôn toàn bộ danh sách partOrderSize cũ đi tránh việc dữ liệu bị ghi đè không đồng bộ với part mới tạo ra
                    var existingPartOrderSizes = await _partOrderSizeRepo.GetAll(existingPart.Id);
                    foreach (var partOrderSize in existingPartOrderSizes)
                    {
                        await _partOrderSizeRepo.Delete(partOrderSize.Id);
                    }
                    await _partRepo.Delete(existingPart.Id);
                }

                foreach (var part in validatedParts)
                {
                    // Thêm mới danh sách production part mới vào hệ thống theo danh sách mới PM gửi lên
                    var newPart = await _partRepo.Create(part);
                    check_parts.Add(newPart);
                    // Lấy danh sách order size theo part đó để tạo mới danh sách part order size tương ứng tránh việc dữ liệu bị ghi đè không đồng bộ với part mới tạo ra
                    foreach (var orderSize in listOrderSizeByCustomer)
                    {
                        var sizeName = await _sizeRepo.GetById(orderSize.SizeId);
                        await _partOrderSizeRepo.Create(new ProductionPartOrderSize
                        {
                            ProductionPartId = newPart.Id,
                            Size = sizeName.Name,
                            Quantity = orderSize.Quantity,
                            Color = orderSize.Color,
                            PartOrderSizeStatusId = PartOrderSizeStatus_Constants.ToDo_ID
                        });
                    }
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


        public async Task<ProductionPartDetailViewDTO> AssignWorkers(int partId,int partOrderSizeId, IEnumerable<int> workerIds)
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

            var partOrderSize = await _partOrderSizeRepo.GetById(partOrderSizeId);
            if(partOrderSize is null)
            {
                throw new ValidationException("Production part size không tồn tại trong hệ thống");
            }
            if (partOrderSize.ProductionPartId != partId)
            {
                throw new ValidationException("Production part order size không khớp production part này");
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
            var updated = await _partAssignRepo.AssignWorkers(partOrderSizeId, workers);
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
                // Thêm Người dùng vào kết quả trả về => Trả về một view gồm 3 khóa ngoại: Part, Team Leader, Assignees
                result.Add(new ProductionPartDetailViewDTO
                {
                    Part = part,
                    // Lấy danh sách Order Size theo PartId
                    ListPartOrderSize = await _partOrderSizeRepo.GetAll(part.Id)
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


        public async Task<IEnumerable<ProductionPartWorkLog>> GetWorkLogs(int partId,int partOrderSizeId)
        {
            _ = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại");
            var partOrderSize = await _partOrderSizeRepo.GetById(partOrderSizeId)
               ?? throw new ValidationException("Production part size không tồn tại trong hệ thống");
            if (partOrderSize.ProductionPartId != partId)
            {
                throw new ValidationException("Production part order size không khớp production part này");
            }
            var logs = await _workLogRepo.GetAll(partOrderSizeId);
            var now = VietnamTime.Now();
            var normalized = new List<ProductionPartWorkLog>();
            foreach (var log in logs)
            {
                log.IsReadOnly = (log.IsReadOnly == true) || now - log.CreateDate > TimeSpan.FromHours(24);
                normalized.Add(log);
            }
            return normalized;
        }

        public async Task<ProductionPartWorkLog> CreateWorkLog(int partId, int partOrderSizeId, int userId, int quantity)
        {
            var productionPart = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại");

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
                var partOrderSize = await _partOrderSizeRepo.GetById(partOrderSizeId); 
                if (partOrderSize is null)
                {
                      throw new ValidationException("Production part size không tồn tại trong hệ thống");
                }
                if(partOrderSize.ProductionPartId != partId)
                {
                    throw new ValidationException("Production part order size không khớp production part này");
                }
                if(!partOrderSize.AssigneeIds.Contains(userId))
                {
                    throw new ValidationException("Người này không được phân công để làm việc này");
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

                // FIX nghiệp vụ ghi nhận sản lượng:
                // Số lượng log phải được khống chế theo ORDER_SIZE tương ứng (màu + size) của partOrderSize hiện tại.
                var orderSizeLimit = await ResolveOrderSizeLimitAsync(getOrder.Id, partOrderSize);
                // Lấy lịch sử submit sản lượng theo đúng partOrderSize hiện tại.
                var allWorkLogsInAPart = await _workLogRepo.GetAll(partOrderSizeId);

                int historyQuantitySubmits = 0;
                foreach (var logpart in allWorkLogsInAPart)
                {
                    historyQuantitySubmits += logpart.Quantity;
                }
                int nowQuantitySubmit = historyQuantitySubmits + quantity;
                // Nếu như đơn hàng chưa hoàn thành thì cập nhật trạng thái thành đang sản xuất
                if (nowQuantitySubmit > 0 && nowQuantitySubmit < orderSizeLimit)
                {
                    productionPart.StatusId = ProductionPart_Constrants.OnGoing_ID;
                }
                if (nowQuantitySubmit > orderSizeLimit)
                {
                    throw new ValidationException("Tổng số lượng làm không thể lớn hơn số lượng đơn hàng giao cho");
                }

                // Update số lượng sản phẩm đã làm vào production part đó
                await _partRepo.Update(productionPart);

                returnData = await _workLogRepo.Create(new ProductionPartWorkLog
                {
                    PartOrderSizeId = partOrderSizeId,
                    UserId = userId,
                    Quantity = quantity,
                    CreateDate = VietnamTime.Now(),
                    IsReadOnly = false,
                    IsPayment = false
                });
            });

            return returnData;
        }

        public async Task<ProductionPartWorkLog> UpdateWorkLog(int partId, int partOrderSizeId, int workLogId, int quantity)
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
                // FIX nghiệp vụ cập nhật sản lượng:
                // Chỉ tổng hợp lịch sử theo đúng PartOrderSize để giới hạn theo ORDER_SIZE (size + màu).
                var historyWorkLogs = await _workLogRepo.GetAll(partOrderSizeId);
                // Tạo data sản lượng đầu ra của các worklog được submit
                int quantityHistoryWorkLog = 0;
                // Lấy thông tin ghi chép của công nhân hiện tại
                var log = await _workLogRepo.GetById(workLogId) ?? throw new ValidationException("Work log không tồn tại");
                if (log.PartOrderSizeId != partOrderSizeId) throw new ValidationException("Work log không thuộc công đoạn này");

                // FIX nghiệp vụ màn hình cập nhật sản lượng:
                // Không cho phép cập nhật khi log đã khóa hoặc đã thanh toán.
                if (log.IsReadOnly)
                {
                    throw new ValidationException("Work log đã bị khóa, không thể cập nhật");
                }
                if (log.IsPayment)
                {
                    throw new ValidationException("Work log đã được thanh toán, không thể cập nhật");
                }
                var partOrderSize = await _partOrderSizeRepo.GetById(partOrderSizeId)
                    ?? throw new ValidationException("Production part size không tồn tại trong hệ thống");
                var orderSizeLimit = await ResolveOrderSizeLimitAsync(getOrder.Id, partOrderSize);


                foreach (var worklog in historyWorkLogs)
                {
                    if (worklog.Id != workLogId)
                    {
                        quantityHistoryWorkLog += worklog.Quantity;
                    }
                }
                int nowQuantitySubmit = quantityHistoryWorkLog + quantity;
                if (nowQuantitySubmit < orderSizeLimit)
                {
                    part.StatusId = ProductionPart_Constrants.OnGoing_ID;
                    log.Quantity = quantity;
                }
               
                if (nowQuantitySubmit > orderSizeLimit)
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
            if (part.StatusId != ProductionPart_Constrants.OnGoing_ID)
            {
                throw new ValidationException("Chỉ có thể hoàn thành công đoạn đang ở trạng thái Đang Sản Xuất");
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
            foreach (var worklog in allQuantityInAPart)
            {
                totalQuantityWorkLog += worklog.Quantity;
            }
            if (totalQuantityWorkLog != order.Quantity)
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

        public async Task<PartPaymentCompletionViewDTO> CompletePartPayment(int partId, int partOrderSizeId, IEnumerable<int> workLogIds)
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
                    if (log.PartOrderSizeId != partOrderSizeId)
                    {
                        throw new ValidationException($"Work log ID = {logId} không thuộc công đoạn hiện tại");
                    }
                   
                    if(log.IsReadOnly == false)
                    {
                        continue;
                    }
                    if (log.IsPayment == true)
                    {
                        continue;
                    }
                    log.IsPayment = true;
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
                    .GroupBy(x => new { x.UserId, Day = DateOnly.FromDateTime(x.CreateDate) })
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
            var logs = (await _workLogRepo.GetAll(null)).Where(x => partIds.Contains(x.Id)).ToList();
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
            var logs = (await _workLogRepo.GetAll(null)).Where(x => partIds.Contains(x.Id)).ToList();
            var workerIds = logs.Select(x => x.UserId).Distinct().ToList();

            var scores = new List<WorkerProductivityScoreViewDTO>();
            foreach (var workerId in workerIds)
            {
                var worker = await _userRepo.GetById(workerId);
                if (worker is null) continue;

                var workerLogs = logs.Where(x => x.UserId == workerId).ToList();
                var totalOutput = workerLogs.Sum(x => x.Quantity);
                var activeDays = workerLogs.Select(x => DateOnly.FromDateTime(x.CreateDate)).Distinct().Count();
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


        // Hàm dùng chung để lấy ngưỡng số lượng tối đa từ ORDER_SIZE theo đúng cặp (Color + Size)
        // phục vụ validate cho màn hình ghi nhận/cập nhật sản lượng.
        private async Task<int> ResolveOrderSizeLimitAsync(int orderId, ProductionPartOrderSize partOrderSize)
        {
            var orderSizes = await _orderSizeRepo.GetAll(orderId);
            // FIX nghiệp vụ đối chiếu ORDER_SIZE:
            // Tìm tất cả bản ghi ORDER_SIZE trùng (màu + size) và cộng tổng số lượng để làm ngưỡng tối đa.
            int resolvedLimit = 0;
            foreach (var orderSize in orderSizes)
            {
                var size = await _sizeRepo.GetById(orderSize.SizeId);
                if (size is null)
                {
                    continue;
                }

                var isMatchedColor = string.Equals(orderSize.Color?.Trim(), partOrderSize.Color?.Trim(), StringComparison.OrdinalIgnoreCase);
                var isMatchedSize = string.Equals(size.Name?.Trim(), partOrderSize.Size?.Trim(), StringComparison.OrdinalIgnoreCase);

                if (isMatchedColor && isMatchedSize)
                {
                    resolvedLimit += orderSize.Quantity;
                }
            }

            if (resolvedLimit <= 0)
            {
                throw new ValidationException("Không tìm thấy ORDER_SIZE tương ứng với màu/size của công đoạn");
            }

            return resolvedLimit;
        }


        //TrungNT: trả về danh sách production work log của một production và hỗ trợ filter theo worker.
        public async Task<IEnumerable<ProductionPartWorkLog>> GetProductionWorkLogs(int productionId, int? workerId)
        {
            _ = await _productionRepo.GetById(productionId) ?? throw new ValidationException("Production không tồn tại");
            var parts = (await _partRepo.GetAll(productionId)).ToList();
            var partIds = parts.Select(x => x.Id).ToHashSet();
            var partOrderSizeIds = new HashSet<int>();
            var partOrderSizeToPartName = new Dictionary<int, string>();
            foreach (var partId in partIds)
            {
                var partName = parts.FirstOrDefault(x => x.Id == partId)?.PartName;
                var partOrderSizes = await _partOrderSizeRepo.GetAll(partId);
                foreach (var partOrderSize in partOrderSizes)
                {
                    partOrderSizeIds.Add(partOrderSize.Id);
                    partOrderSizeToPartName[partOrderSize.Id] = partName ?? string.Empty;
                }
            }

            var logs = (await _workLogRepo.GetAll(null))
                .Where(x => partOrderSizeIds.Contains(x.PartOrderSizeId))
                .ToList();
            var users = (await _userRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.FullName);

            if (workerId.HasValue)
            {
                logs = logs.Where(x => x.UserId == workerId.Value).ToList();
            }

            return logs
                .OrderByDescending(x => x.CreateDate)
                .Select(x =>
                {
                    x.PartName = partOrderSizeToPartName.TryGetValue(x.PartOrderSizeId, out var partName) ? partName : null;
                    x.WorkerName = users.TryGetValue(x.UserId, out var workerName) ? workerName : null;
                    return x;
                })
                .ToList();
        }


        //TrungNT: xóa work log khi bản ghi chưa bị khóa (IsReadOnly = false) và chưa thanh toán.
        public async Task DeleteWorkLog(int workLogId)
        {
            var workLog = await _workLogRepo.GetById(workLogId) ?? throw new ValidationException("Work log không tồn tại");

            if (workLog.IsReadOnly)
            {
                throw new ValidationException("Work log đã khóa, không thể xóa");
            }
            if (workLog.IsPayment)
            {
                throw new ValidationException("Work log đã thanh toán, không thể xóa");
            }
            var partOrderSize = await _partOrderSizeRepo.GetById(workLog.PartOrderSizeId)
                ?? throw new ValidationException("Production part size không tồn tại");
            var part = await _partRepo.GetById(partOrderSize.ProductionPartId)
                ?? throw new ValidationException("Production part không tồn tại");
            await _workLogRepo.Delete(workLogId);
        }

        // Mục đích: PM duyệt lại số lượng thực tế của work log và khóa bản ghi để tránh sửa lại.
        public async Task<ProductionPartWorkLog> ApproveWorkLog(int partId, int partOrderSizeId, int workLogId, int approvedQuantity)
        {
            if (approvedQuantity < 0) throw new ValidationException("Số lượng nghiệm thu phải >= 0");
            ProductionPartWorkLog approvedLog = null!;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var part = await _partRepo.GetById(partId) ?? throw new ValidationException("Production part không tồn tại");
                var partOrderSize = await _partOrderSizeRepo.GetById(partOrderSizeId) ?? throw new ValidationException("Production part size không tồn tại");
                if (partOrderSize.ProductionPartId != partId) throw new ValidationException("Production part order size không khớp production part này");

                var log = await _workLogRepo.GetById(workLogId) ?? throw new ValidationException("Work log không tồn tại");
                if (log.PartOrderSizeId != partOrderSizeId) throw new ValidationException("Work log không thuộc production part size này");
                if (log.IsPayment) throw new ValidationException("Work log đã thanh toán, không thể nghiệm thu lại");
                if (log.IsReadOnly) throw new ValidationException("Work log đã khóa, không thể nghiệm thu lại");
                if (approvedQuantity > log.Quantity) throw new ValidationException("Số lượng nghiệm thu không thể lớn hơn số lượng đã submit");

                var production = await _productionRepo.GetById(part.ProductionId) ?? throw new ValidationException("Production không tồn tại");
                var order = await _orderRepo.GetById(production.OrderId) ?? throw new ValidationException("Order không tồn tại");
                var orderSizeLimit = await ResolveOrderSizeLimitAsync(order.Id, partOrderSize);

                var allLogs = (await _workLogRepo.GetAll(partOrderSizeId)).ToList();
                var totalWithoutCurrent = allLogs.Where(x => x.Id != workLogId).Sum(x => x.Quantity);
                var approvedTotal = totalWithoutCurrent + approvedQuantity;
                if (approvedTotal > orderSizeLimit)
                {
                    throw new ValidationException("Số lượng nghiệm thu vượt quá số lượng order size");
                }

                log.Quantity = approvedQuantity;
                log.IsReadOnly = true;
                approvedLog = await _workLogRepo.Update(log);

                // Kiểm tra xem các lô hàng khác đang xong ở công đoạn đấy chưa
                var getAllPartOrderSizes = await _partOrderSizeRepo.GetAll(part.Id);
                bool checkOtherPartOrderSizeStatus = getAllPartOrderSizes.Any(x => x.Id != partOrderSizeId && x.PartOrderSizeStatusId != PartOrderSizeStatus_Constants.Done_ID);

                part.StatusId = (approvedTotal == orderSizeLimit && !checkOtherPartOrderSizeStatus)
                    ? ProductionPart_Constrants.Done_ID
                    : ProductionPart_Constrants.OnGoing_ID;

                partOrderSize.PartOrderSizeStatusId = approvedTotal == orderSizeLimit ? PartOrderSizeStatus_Constants.Done_ID : PartOrderSizeStatus_Constants.OnGoing_ID;

                await _partRepo.Update(part);
                await _partOrderSizeRepo.Update(partOrderSize);
            });
            return approvedLog;
        }

        // Mục đích: đổi trạng thái issue theo workflow ToDo -> Processing -> Fixed hoặc ToDo -> Error.
        public async Task<ProductionIssueLog> UpdateIssueStatus(int issueId, int statusId)
        {
            var issue = await _issueRepo.GetById(issueId) ?? throw new ValidationException("Issue không tồn tại");

            var isValidTransition =
                (issue.StatusId == IssueStatus_Constrants.ToDo_ID && (statusId == IssueStatus_Constrants.Processing_ID || statusId == IssueStatus_Constrants.Error_ID)) ||
                (issue.StatusId == IssueStatus_Constrants.Processing_ID && statusId == IssueStatus_Constrants.Fixed_ID) ||
                issue.StatusId == statusId;

            if (!isValidTransition)
            {
                throw new ValidationException("Trạng thái issue không hợp lệ theo workflow");
            }

            issue.StatusId = statusId;
            return await _issueRepo.Update(issue);
        }

        // Mục đích: xác nhận issue không thể sửa và đồng bộ trừ sản lượng của part size + order tổng.

        // Mục đích: xác nhận issue không thể sửa và đồng bộ trừ sản lượng của part size + order tổng.
        public async Task<ProductionIssueLog> ConfirmUnfixableIssue(int issueId, int confirmedQuantity)
        {
            if (confirmedQuantity <= 0) throw new ValidationException("Số lượng lỗi xác nhận phải > 0");
            ProductionIssueLog updatedIssue = null!;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var issue = await _issueRepo.GetById(issueId) ?? throw new ValidationException("Issue không tồn tại");
                var partOrderSize = await _partOrderSizeRepo.GetById(issue.PartOrderSizeId) ?? throw new ValidationException("Production part size không tồn tại");
                var part = await _partRepo.GetById(partOrderSize.ProductionPartId) ?? throw new ValidationException("Production part không tồn tại");
                var production = await _productionRepo.GetById(part.ProductionId) ?? throw new ValidationException("Production không tồn tại");
                var order = await _orderRepo.GetById(production.OrderId) ?? throw new ValidationException("Order không tồn tại");

                if (issue.StatusId != IssueStatus_Constrants.Error_ID)
                {
                    throw new ValidationException("Lỗi phải ở trạng thái 'Không thể sửa' trước khi xác nhận");
                }

                if (confirmedQuantity > issue.Quantity)
                {
                    throw new ValidationException("Số lượng xác nhận không thể lớn hơn số lượng lỗi đã báo");
                }

                var relatedPartSizes = new List<ProductionPartOrderSize>();
                var allParts = await _partRepo.GetAll(production.Id);
                foreach (var productionPart in allParts)
                {
                    var sizes = await _partOrderSizeRepo.GetAll(productionPart.Id);
                    relatedPartSizes.AddRange(sizes.Where(x =>
                        string.Equals(x.Color?.Trim(), partOrderSize.Color?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Size?.Trim(), partOrderSize.Size?.Trim(), StringComparison.OrdinalIgnoreCase)));
                }

                foreach (var relatedPartSize in relatedPartSizes)
                {
                    if (relatedPartSize.Quantity < confirmedQuantity)
                    {
                        throw new ValidationException("Số lượng công đoạn hiện tại không đủ để trừ theo issue");
                    }
                    relatedPartSize.Quantity -= confirmedQuantity;
                    await _partOrderSizeRepo.Update(relatedPartSize);
                }

                // Khóa toàn bộ work log của partOrderSize lỗi để tránh phát sinh thêm sau khi đã chốt "không thể sửa".
                var relatedWorkLogs = await _workLogRepo.GetAll(issue.PartOrderSizeId);
                foreach (var workLog in relatedWorkLogs)
                {
                    workLog.IsReadOnly = true;
                    await _workLogRepo.Update(workLog);
                }

                // Trừ số lượng ORDER_SIZE liên quan (cùng màu + size với partOrderSize của issue).
                // Vì một đơn có thể có nhiều dòng ORDER_SIZE cùng màu/size, hệ thống sẽ phân bổ trừ lần lượt.
                var orderSizes = (await _orderSizeRepo.GetAll(order.Id)).ToList();

                // Do OrderSize domain chỉ có SizeId, cần map size-name từ bảng Size để so khớp với partOrderSize.Size.
                var matchedOrderSizes = new List<OrderSize>();
                foreach (var orderSize in orderSizes)
                {
                    var size = await _sizeRepo.GetById(orderSize.SizeId);
                    var isMatchedColor = string.Equals(orderSize.Color?.Trim(), partOrderSize.Color?.Trim(), StringComparison.OrdinalIgnoreCase);
                    var isMatchedSize = string.Equals(size?.Name?.Trim(), partOrderSize.Size?.Trim(), StringComparison.OrdinalIgnoreCase);
                    if (isMatchedColor && isMatchedSize)
                    {
                        matchedOrderSizes.Add(orderSize);
                    }
                }

                if (!matchedOrderSizes.Any())
                {
                    throw new ValidationException("Không tìm thấy lỗi liên quan để trừ theo issue");
                }

                var remainToDeduct = confirmedQuantity;
                foreach (var matchedOrderSize in matchedOrderSizes.OrderBy(x => x.Id))
                {
                    if (remainToDeduct <= 0) break;
                    var deduct = Math.Min(matchedOrderSize.Quantity, remainToDeduct);
                    matchedOrderSize.Quantity -= deduct;
                    remainToDeduct -= deduct;
                    await _orderSizeRepo.Update(matchedOrderSize);
                }

                if (remainToDeduct > 0)
                {
                    throw new ValidationException("Tổng số lượng trong đơn hàng liên quan không đủ để trừ theo lỗi");
                }

                order.Quantity -= confirmedQuantity;
                if (order.Quantity < 0) throw new ValidationException("Tổng số lượng order không hợp lệ sau khi trừ issue");
                await _orderRepo.Update(order);

                issue.Quantity = confirmedQuantity;
                issue.StatusId = IssueStatus_Constrants.Error_ID;
                updatedIssue = await _issueRepo.Update(issue);
            });

            return updatedIssue;
        }

        // Mục đích: lấy danh sách delivery theo user để hiển thị lịch sử gửi hàng.
        public async Task<IEnumerable<Delivery>> GetDeliveriesByOrder(int orderId)
        {
            _ = await _orderRepo.GetById(orderId) ?? throw new ValidationException("Order không tồn tại");
            var orderSizes = (await _orderSizeRepo.GetAll(orderId)).ToList();
            var orderSizeIds = orderSizes.Select(x => x.Id).ToHashSet();
            var sizeLookup = (await _sizeRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.Name);

            var deliveries = await _deliveryRepo.GetAll(null);
            var matchedDeliveries = deliveries.Where(x => orderSizeIds.Contains(x.OrderSizeId)).ToList();

            var orderSizeLookup = orderSizes.ToDictionary(x => x.Id);
            foreach (var delivery in matchedDeliveries)
            {
                if (!orderSizeLookup.TryGetValue(delivery.OrderSizeId, out var orderSize)) continue;
                delivery.Color = orderSize.Color;
                delivery.SizeName = sizeLookup.TryGetValue(orderSize.SizeId, out var sizeName) ? sizeName : null;
            }

            return matchedDeliveries;
        }

        // Mục đích: tạo nhiều delivery cho một order theo danh sách order_size được chọn.
        public async Task<IEnumerable<Delivery>> CreateDeliveries(int orderId, IEnumerable<Delivery> deliveries)
        {
            var order = await _orderRepo.GetById(orderId) ?? throw new ValidationException("Order không tồn tại");
            var payload = (deliveries ?? Enumerable.Empty<Delivery>()).ToList();
            if (payload.Count == 0) throw new ValidationException("Danh sách delivery không hợp lệ");



            //Trung Fixx - 14-04-26

            var orderSizes = (await _orderSizeRepo.GetAll(orderId)).ToList();
            var sizeLookup = (await _sizeRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.Name);
            var orderSizeLookup = orderSizes.ToDictionary(x => x.Id);
            var orderSizeIds = orderSizes.Select(x => x.Id).ToHashSet();

            // Tổng số lượng đã giao/đang giao theo (màu, size): chỉ lấy trạng thái ToDo + Done.
            var existingDeliveries = (await _deliveryRepo.GetAll(null))
                .Where(x => orderSizeIds.Contains(x.OrderSizeId) &&
                            (x.DeliverStatusId == DeliveryStatus_Constrants.ToDo_ID ||
                             x.DeliverStatusId == DeliveryStatus_Constrants.Done_ID))
                .ToList();

            var deliveredOrShippingByKey = existingDeliveries
                .Where(x => orderSizeLookup.ContainsKey(x.OrderSizeId))
                .GroupBy(x =>
                {
                    var os = orderSizeLookup[x.OrderSizeId];
                    var sizeName = sizeLookup.TryGetValue(os.SizeId, out var mappedSize) ? mappedSize : string.Empty;
                    return $"{os.Color?.Trim()?.ToLowerInvariant()}|{sizeName.Trim().ToLowerInvariant()}";
                })
                .ToDictionary(x => x.Key, x => x.Sum(y => y.DeliverQuantity));

            // Số lượng đã hoàn thành theo (màu, size) từ work log đã nghiệm thu của công đoạn cuối.
            var productions = (await _productionRepo.GetAll(null))
                .Where(x => x.OrderId == orderId)
                .ToList();
            var completedByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var production in productions)
            {
                var lastPart = (await _partRepo.GetAll(production.Id)).OrderByDescending(x => x.Id).FirstOrDefault();
                if (lastPart is null) continue;

                var finalPartOrderSizes = (await _partOrderSizeRepo.GetAll(lastPart.Id)).ToList();
                foreach (var partOrderSize in finalPartOrderSizes)
                {
                    var key = $"{partOrderSize.Color?.Trim()?.ToLowerInvariant()}|{partOrderSize.Size?.Trim()?.ToLowerInvariant()}";
                    var approvedWorkLogQuantity = (await _workLogRepo.GetAll(partOrderSize.Id))
                        .Where(x => x.IsReadOnly)
                        .Sum(x => Math.Max(0, x.Quantity));

                    if (!completedByKey.ContainsKey(key))
                    {
                        completedByKey[key] = 0;
                    }
                    completedByKey[key] += approvedWorkLogQuantity;
                }
            }
            // Tổng số lượng chuẩn bị giao theo (màu, size) từ payload.
            var requestedByKey = payload
                .GroupBy(x =>
                {
                    if (!orderSizeLookup.TryGetValue(x.OrderSizeId, out var os))
                    {
                        throw new ValidationException($"Đã Có Lỗi Xảy Ra Công Đoạn Không Khớp Nhau");
                    }
                    var sizeName = sizeLookup.TryGetValue(os.SizeId, out var mappedSize) ? mappedSize : string.Empty;
                    return $"{os.Color?.Trim()?.ToLowerInvariant()}|{sizeName.Trim().ToLowerInvariant()}";
                })
                .ToDictionary(x => x.Key, x => x.Sum(y => y.DeliverQuantity));

            foreach (var requested in requestedByKey)
            {
                var completed = completedByKey.TryGetValue(requested.Key, out var completedQuantity) ? completedQuantity : 0;
                var deliveredOrShipping = deliveredOrShippingByKey.TryGetValue(requested.Key, out var deliveredQuantity) ? deliveredQuantity : 0;
                var availableToShip = completed - deliveredOrShipping;

                if (availableToShip <= 0)
                {
                    throw new ValidationException("Số lượng có thể giao không còn (đã giao hoặc đang giao hết)");
                }

                if (requested.Value > availableToShip)
                {
                    throw new ValidationException("Tổng số lượng chuẩn bị giao vượt quá số lượng còn có thể giao");
                }
            }
            //

            var created = new List<Delivery>();

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var delivery in payload)
                {
                    var matchedOrderSize = orderSizes.FirstOrDefault(x => x.Id == delivery.OrderSizeId);
                    if (matchedOrderSize is null)
                    {
                        throw new ValidationException($"OrderSizeId = {delivery.OrderSizeId} không thuộc OrderId = {orderId}");
                    }

                    if (delivery.DeliverQuantity <= 0)
                    {
                        throw new ValidationException("Số lượng giao phải > 0");
                    }

                    var createdDelivery = await _deliveryRepo.Create(new Delivery
                    {
                        OrderSizeId = delivery.OrderSizeId,
                        DeliverQuantity = delivery.DeliverQuantity,
                        DeliveredAt = VietnamTime.Now(),
                        DeliverStatusId = DeliveryStatus_Constrants.ToDo_ID,
                        ReceivedDate = null
                    });
                    created.Add(createdDelivery);
                }
            });

            return created;
        }


        // Mục đích: customer xác nhận đã nhận/chưa nhận delivery theo cơ chế xác thực 2 bước Yes/No; Guest auto nhận hàng.
        public async Task<Delivery> ConfirmDeliveryReceipt(int deliveryId, string confirmationText)
        {
            if (deliveryId <= 0) throw new ValidationException("DeliveryId không hợp lệ");
            if (string.IsNullOrWhiteSpace(confirmationText)) throw new ValidationException("Thiếu thông tin xác nhận delivery");

            var normalized = confirmationText.Trim();
            if (!normalized.Equals("Yes", StringComparison.OrdinalIgnoreCase) &&
                !normalized.Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Giá trị xác nhận chỉ chấp nhận Yes hoặc No");
            }

            var delivery = await _deliveryRepo.GetById(deliveryId) ?? throw new ValidationException("Delivery không tồn tại");
            var orderSize = await _orderSizeRepo.GetById(delivery.OrderSizeId) ?? throw new ValidationException("Order size không tồn tại");
            var order = await _orderRepo.GetById(orderSize.OrderId) ?? throw new ValidationException("Order không tồn tại");

            if(delivery.DeliverStatusId != DeliveryStatus_Constrants.ToDo_ID)
            {
                throw new ValidationException("Đơn hàng đã được xem xét bởi khách hàng");
            }

            // Guest order: tự động xác nhận đã nhận hàng.
            if (order.GuestId.HasValue)
            {
                delivery.DeliverStatusId = DeliveryStatus_Constrants.Done_ID;
                delivery.ReceivedDate = VietnamTime.Now();
                return await _deliveryRepo.Update(delivery);
            }

            delivery.DeliverStatusId = normalized.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                ? DeliveryStatus_Constrants.Done_ID
                : DeliveryStatus_Constrants.NotYet_ID;
            delivery.ReceivedDate = VietnamTime.Now();
            return await _deliveryRepo.Update(delivery);
        }



        // Mục đích: dữ liệu cho màn hình ghi nhận giao hàng theo (màu, size) với giới hạn tối đa giao đợt này.
        public async Task<IEnumerable<DeliveryPlanningItemViewDTO>> GetDeliveryPlanningByOrder(int orderId)
        {
            _ = await _orderRepo.GetById(orderId) ?? throw new ValidationException("Order không tồn tại");

            var orderSizes = (await _orderSizeRepo.GetAll(orderId)).ToList();
            var orderSizeIds = orderSizes.Select(x => x.Id).ToHashSet();
            var sizeLookup = (await _sizeRepo.GetAll(null)).ToDictionary(x => x.Id, x => x.Name);

            var deliveries = (await _deliveryRepo.GetAll(null)).Where(x => orderSizeIds.Contains(x.OrderSizeId)).ToList();
            var deliveredLookup = deliveries
                .GroupBy(x => x.OrderSizeId)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.DeliverQuantity));

            // Lấy toàn bộ production đang làm cho order để tính completed từ work log đã nghiệm thu ở công đoạn cuối.
            var productions = (await _productionRepo.GetAll(null))
                .Where(x => x.OrderId == orderId)
                .ToList();
            var completedLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var production in productions)
            {
                // Lấy công đoạn cuối cùng (id lớn nhất) trong từng production.
                var parts = (await _partRepo.GetAll(production.Id)).OrderByDescending(x => x.Id).ToList();
                var lastPart = parts.FirstOrDefault();

                if (lastPart is null)
                {
                    continue;
                }

                var finalPartOrderSizes = (await _partOrderSizeRepo.GetAll(lastPart.Id)).ToList();
                foreach (var partOrderSize in finalPartOrderSizes)
                {
                    var key = $"{partOrderSize.Color?.Trim()?.ToLowerInvariant()}|{partOrderSize.Size?.Trim()?.ToLowerInvariant()}";
                    if (!completedLookup.ContainsKey(key))
                    {
                        completedLookup[key] = 0;
                    }

                    var approvedWorkLogQuantity = (await _workLogRepo.GetAll(partOrderSize.Id))
                        .Where(x => x.IsReadOnly)
                        .Sum(x => Math.Max(0, x.Quantity));

                    completedLookup[key] += approvedWorkLogQuantity;
                }
            }

            return orderSizes
                .OrderBy(x => x.Id)
                .Select(orderSize =>
                {
                    var sizeName = sizeLookup.TryGetValue(orderSize.SizeId, out var mappedSize) ? mappedSize : string.Empty;
                    var delivered = deliveredLookup.TryGetValue(orderSize.Id, out var totalDelivered) ? totalDelivered : 0;
                    var remaining = Math.Max(0, orderSize.Quantity - delivered);

                    var colorKey = orderSize.Color?.Trim()?.ToLowerInvariant() ?? string.Empty;
                    var sizeKey = sizeName.Trim().ToLowerInvariant();
                    var completedByFinalPart = completedLookup.TryGetValue($"{colorKey}|{sizeKey}", out var completed) ? completed : 0;
                    var maxDeliverable = Math.Max(0, Math.Min(remaining, completedByFinalPart - delivered));

                    return new DeliveryPlanningItemViewDTO
                    {
                        OrderSizeId = orderSize.Id,
                        Color = orderSize.Color,
                        SizeName = sizeName,
                        TotalOrderedQuantity = orderSize.Quantity,
                        DeliveredQuantity = delivered,
                        RemainingQuantity = remaining,
                        CompletedQuantity = completedByFinalPart,
                        MaxDeliverableQuantity = maxDeliverable
                    };
                });
        }


    }
}
