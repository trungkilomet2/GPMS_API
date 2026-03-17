using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System.ComponentModel.DataAnnotations;

namespace GPMS.APPLICATION.Services
{
    public class ProductionPartService : IProductionPartRepositories
    {
        private readonly IBaseRepositories<ProductionPart> _partRepo;
        private readonly IBaseRepositories<Production> _productionRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductionPartService(
            IBaseRepositories<ProductionPart> partRepo,
            IBaseRepositories<Production> productionRepo,
            IBaseRepositories<User> userRepo,
            IUnitOfWork unitOfWork)
        {
            _partRepo = partRepo;
            _productionRepo = productionRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
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
            existing.TeamLeaderId = part.TeamLeaderId;
            existing.Cpu = part.Cpu;
            existing.StatusId = part.StatusId;
            existing.StartDate = part.StartDate;
            existing.EndDate = part.EndDate;

            var updated = await _partRepo.Update(existing);
            return (await BuildViews(new[] { updated })).First();
        }
        
        // Hiện tại đang chưa sử dụng
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
            var updated = new ProductionPartDetailViewDTO();
            //var updated = await _partRepo.AssignWorkers(partId, workers);
            return null;
            //return (await BuildViews(new[] { updated })).First();
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
                throw new ValidationException("Production part data is required");
            }

            if (string.IsNullOrWhiteSpace(part.PartName))
            {
                throw new ValidationException("PartName không được để trống");
            }

            if (part.TeamLeaderId <= 0)
            {
                throw new ValidationException("TeamLeaderId phải > 0");
            }

            var teamLeader = await _userRepo.GetById(part.TeamLeaderId);
            if (teamLeader is null)
            {
                throw new ValidationException("Team leader không tồn tại trong hệ thống");
            }

            if (part.Cpu <= 0)
            {
                throw new ValidationException("Cpu phải > 0");
            }

            if (part.StatusId <= 0)
            {
                throw new ValidationException("StatusId phải > 0");
            }

            if (part.StartDate.HasValue && part.EndDate.HasValue && part.EndDate < part.StartDate)
            {
                throw new ValidationException("EndDate phải lớn hơn hoặc bằng StartDate");
            }
        }

        // Hàm tổng quát dùng để trả về một Production Part Detail
        private async Task<IEnumerable<ProductionPartDetailViewDTO>> BuildViews(IEnumerable<ProductionPart> parts)
        {
            var result = new List<ProductionPartDetailViewDTO>();

            foreach (var part in parts)
            {
                // Lấy thông tin chi tiết của Part, bao gồm cả Team Leader và Assignees
                var detail = await _partRepo.GetById(part.Id);
                int teamLeaderId = detail.TeamLeaderId;
                // Lấy thông tin của Team Leader
                var leader = await _userRepo.GetById(detail.TeamLeaderId);
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
                    TeamLeader = leader,
                    Assignees = assigneeUsers
                });
            }

            return result;
        }
    }
}
