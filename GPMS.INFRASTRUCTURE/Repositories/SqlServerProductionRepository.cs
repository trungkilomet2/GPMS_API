using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionRepository : IBaseProductionRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Production> CreateProduction(Production production)
        {
            var isUserExist = await _context.USER.AnyAsync(u => u.USER_ID == production.PmId);
            if(!isUserExist)
            {
                throw new Exception($"User with id '{production.PmId}' not found.");
                return null;
            }
            var isOrderExist = await _context.ORDER.AnyAsync(o => o.ORDER_ID == production.OrderId);
            if (!isOrderExist)
            {
                throw new Exception($"User with id '{production.OrderId}' not found.");
                return null;
            }
            var entity = _mapper.Map<PRODUCTION>(production);
            _context.PRODUCTION.Add(entity);
            await _context.SaveChangesAsync();

            return await GetProductionDetail(entity.PRODUCTION_ID) ?? throw new Exception("Failed to create production.");
        }

        public async Task<IEnumerable<Production>> GetProductionList()
            => _mapper.Map<IEnumerable<Production>>(await QueryProductions().ToListAsync());

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
    }
}
