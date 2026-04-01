using AutoMapper;
using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionRejectRepository : IBaseRepositories<ProductionRejectReason>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionRejectRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductionRejectReason> Create(ProductionRejectReason entity)
        {
            var existing = await _context.PRODUCTION_REJECT_REASON.FirstOrDefaultAsync(x => x.PRODUCTION_ID == entity.ProductionId);
            if (existing is not null)
            {
                existing.USER_ID = entity.UserId;
                existing.REASON = entity.Reason;
                existing.CREATED_AT = VietnamTime.Now();
                await _context.SaveChangesAsync();
                return _mapper.Map<ProductionRejectReason>(existing);
            }

            var db = _mapper.Map<PRODUCTION_REJECT_REASON>(entity);
            await _context.PRODUCTION_REJECT_REASON.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductionRejectReason>(db);
        }

        public Task Delete(object id) => throw new NotImplementedException();

        public async Task<IEnumerable<ProductionRejectReason>> GetAll(object? obj)
        {
            var data = await _context.PRODUCTION_REJECT_REASON.ToListAsync();
            return _mapper.Map<IEnumerable<ProductionRejectReason>>(data);
        }

        public async Task<ProductionRejectReason> GetById(object id)
        {
            if (id is not int productionId) return null;
            var data = await _context.PRODUCTION_REJECT_REASON.FirstOrDefaultAsync(x => x.PRODUCTION_ID == productionId);
            return data is null ? null : _mapper.Map<ProductionRejectReason>(data);
        }

        public Task<ProductionRejectReason> Update(ProductionRejectReason entity) => Create(entity);
    }
}