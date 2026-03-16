using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionRepository : IBaseRepositories<Production>, IBaseProductionRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<IEnumerable<Production>> GetAll(object? obj)
        {
            if (obj is null)
            {
                var data = _context.PRODUCTION.Include(p => p.ORDER);
                return Task.FromResult(_mapper.Map<IEnumerable<Production>>(data));
            }
            throw new Exception("Đã có lỗi xảy ra trong hệ thống");
        }

        public Task<Production> GetById(object id)
        {
            if (id is int)
            {
                var data = _context.PRODUCTION.Where(p => p.PRODUCTION_ID == (int)id).FirstOrDefault();
                return data is null ? Task.FromResult<Production>(null) : Task.FromResult(_mapper.Map<Production>(data));
            }
            throw new Exception("Đã có lỗi xảy ra trong hệ thống");
        }

        public async Task<Production> Create(Production entity)
        {
            var pm_id = await _context.USER.Include(u => u.ROLE).Where(u => u.USER_ID == entity.PmId).FirstOrDefaultAsync();
            if (pm_id is null)
            {
                throw new Exception($"User with id '{entity.PmId}' not found.");
                return null;
            }

            var isOrderExist = await _context.ORDER.AnyAsync(o => o.ORDER_ID == entity.OrderId);
            if (!isOrderExist)
            {
                throw new Exception($"User with id '{entity.OrderId}' not found.");
                return null;
            }
            var production_database = _mapper.Map<PRODUCTION>(entity);
            _context.PRODUCTION.Add(production_database);
            await _context.SaveChangesAsync();
            return await GetById(production_database.PRODUCTION_ID) ?? throw new Exception("Xảy ra lỗi khi tạo đơn hàng");
        }

        public Task<Production> Update(Production entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Production>> GetProductionList()
            => _mapper.Map<IEnumerable<Production>>(await QueryProductions()
                .Include(p=> p.ORDER).ThenInclude(p=> p.O_TEMPLATE)
                .Include(p=> p.ORDER).ThenInclude(p=> p.O_MATERIAL)
                .ToListAsync());

        public async Task<Production?> GetProductionDetail(int productionId)
        {
            var entity = await QueryProductions().FirstOrDefaultAsync(p => p.PRODUCTION_ID == productionId);
            return entity is null ? null : _mapper.Map<Production>(entity);
        }

        public async Task<Production> UpdateProduction(Production production)
        {
            var entity = await _context.PRODUCTION.FirstOrDefaultAsync(p => p.PRODUCTION_ID == production.Id)
                ?? throw new Exception($"Production with id '{production.Id}' not found.");
            entity.PM_ID = production.PmId;
            entity.ORDER_ID = production.OrderId;
            entity.P_START_DATE = production.StartDate;
            entity.P_END_DATE = production.EndDate;
            entity.PS_ID = production.StatusId;
            await _context.SaveChangesAsync();
            return await GetProductionDetail(production.Id) ?? throw new Exception("Failed to update production.");
        }

        public async Task<int> GetStatusIdByName(string statusName)
        {
            var status = await _context.P_STATUS.FirstOrDefaultAsync(x => x.NAME == statusName);
            if (status != null) return status.PS_ID;

            var fallback = await _context.P_STATUS.FirstOrDefaultAsync(x => x.NAME == ProductionStatus_Constants.NeedUpdate)
                ?? await _context.P_STATUS.FirstOrDefaultAsync();

            return fallback?.PS_ID ?? throw new Exception("No production status configured.");
        }

        public async Task SaveRejectReason(int productionId, int userId, string reason)
        {
            var current = await _context.PRODUCTION_REJECT_REASON.FirstOrDefaultAsync(x => x.PRODUCTION_ID == productionId);
            if (current is null)
            {
                _context.PRODUCTION_REJECT_REASON.Add(new PRODUCTION_REJECT_REASON
                {
                    PRODUCTION_ID = productionId,
                    USER_ID = userId,
                    REASON = reason,
                    CREATED_AT = DateTime.Now
                });
            }
            else
            {
                current.USER_ID = userId;
                current.REASON = reason;
                current.CREATED_AT = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Production>> GetPendingProductionPlans()
        {
            var productions = await QueryProductions()
                .Where(p => !p.P_PART.Any())
                .ToListAsync();
            return _mapper.Map<IEnumerable<Production>>(productions);
        }

        public async Task<IEnumerable<Production>> GetProductionPlanList()
        {
            var productions = await QueryProductions()
                .Where(p => p.P_PART.Any())
                .ToListAsync();
            return _mapper.Map<IEnumerable<Production>>(productions);
        }

        public async Task ReplaceProductionParts(int productionId, IEnumerable<ProductionPart> parts)
        {
            var existing = _context.P_PART.Where(p => p.PRODUCTION_ID == productionId);
            _context.P_PART.RemoveRange(existing);
            await _context.SaveChangesAsync();

            var defaultPartStatusId = (await _context.P_PART_STATUS.FirstOrDefaultAsync())?.PPS_ID
                ?? throw new Exception("No production part status configured.");

            foreach (var part in parts)
            {
                _context.P_PART.Add(new P_PART
                {
                    PRODUCTION_ID = productionId,
                    PART_NAME = part.PartName,
                    TEAM_LEADER_ID = part.TeamLeaderId,
                    START_DATE = part.StartDate,
                    END_DATE = part.EndDate,
                    CPU = part.Cpu,
                    PPS_ID = part.StatusId == 0 ? defaultPartStatusId : part.StatusId,
                });
            }
            await _context.SaveChangesAsync();
        }

        private IQueryable<PRODUCTION> QueryProductions() => _context.PRODUCTION
            .Include(x => x.PS)
            .Include(x => x.P_PART)
                .ThenInclude(p => p.PPS)
            .Include(x => x.PRODUCTION_REJECT_REASON);

        public Task<Production> CreateProduction(Production production)
        {
            throw new NotImplementedException();
        }
    }
}
