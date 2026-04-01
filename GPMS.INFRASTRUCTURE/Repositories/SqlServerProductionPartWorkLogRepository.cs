using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionPartWorkLogRepository : IBaseRepositories<ProductionPartWorkLog>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionPartWorkLogRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductionPartWorkLog> Create(ProductionPartWorkLog entity)
        {
            var db = _mapper.Map<PART_WORK_LOG>(entity);
            await _context.PART_WORK_LOG.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductionPartWorkLog>(db);
        }

        public async Task Delete(object id)
        {
            if (id is not int logId) return;
            var db = await _context.PART_WORK_LOG.FirstOrDefaultAsync(x => x.WL_ID == logId);
            if (db is null) return;
            _context.PART_WORK_LOG.Remove(db);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductionPartWorkLog>> GetAll(object? obj)
        {
            IQueryable<PART_WORK_LOG> query = _context.PART_WORK_LOG;
            if (obj is int partId)
            {
                query = query.Where(x => x.PP_ID == partId);
            }
            return _mapper.Map<IEnumerable<ProductionPartWorkLog>>(await query.OrderByDescending(x => x.CREATE_DATE).ToListAsync());
        }

        public async Task<ProductionPartWorkLog> GetById(object id)
        {
            if (id is not int logId) return null;
            var data = await _context.PART_WORK_LOG.FirstOrDefaultAsync(x => x.WL_ID == logId);
            return data is null ? null : _mapper.Map<ProductionPartWorkLog>(data);
        }

        public async Task<ProductionPartWorkLog> Update(ProductionPartWorkLog entity)
        {
            var db = await _context.PART_WORK_LOG.FirstOrDefaultAsync(x => x.WL_ID == entity.Id);
            if (db is null) throw new KeyNotFoundException("Work log not found");
            db.QUANTITY = entity.Quantity;
            db.IS_READ_ONLY = entity.IsReadOnly;
            db.IS_PAYMENT = entity.IsPayment;
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductionPartWorkLog>(db);
        }

    }
}

   