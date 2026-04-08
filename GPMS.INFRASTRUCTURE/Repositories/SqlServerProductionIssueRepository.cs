using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionIssueRepository : IBaseRepositories<ProductionIssueLog>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerProductionIssueRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductionIssueLog> Create(ProductionIssueLog entity)
        {
            var db = _mapper.Map<PRODUCTION_ISSUE_LOG>(entity);
            await _context.PRODUCTION_ISSUE_LOG.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductionIssueLog>(db);
        }

        public Task Delete(object id) => throw new NotImplementedException();

        public async Task<IEnumerable<ProductionIssueLog>> GetAll(object? obj)
        {
            IQueryable<PRODUCTION_ISSUE_LOG> query = _context.PRODUCTION_ISSUE_LOG;
            if (obj is int productionId)
            {
                query = query.Where(x => x.PPOS.PP.PRODUCTION_ID == productionId);
            }

            var data = await query.OrderByDescending(x => x.CREATED_AT).ToListAsync();
            return _mapper.Map<IEnumerable<ProductionIssueLog>>(data);
        }

        public async Task<ProductionIssueLog> GetById(object id)
        {
            if (id is not int issueId) return null;
            var data = await _context.PRODUCTION_ISSUE_LOG.FirstOrDefaultAsync(x => x.ISSUE_ID == issueId);
            return data is null ? null : _mapper.Map<ProductionIssueLog>(data);
        }

        public async Task<ProductionIssueLog> Update(ProductionIssueLog entity)
        {
            var db = await _context.PRODUCTION_ISSUE_LOG.FirstOrDefaultAsync(x => x.ISSUE_ID == entity.Id);
            if (db is null) throw new KeyNotFoundException("Issue not found");
            db.TITLE = entity.Title;
            db.DESCRIPTION = entity.Description;
            db.QUANTITY = entity.Quantity;
            db.PRIORITY = entity.Priority;
            db.IS_ID = entity.StatusId;
            db.ASSIGNED_TO = entity.AssignedTo ?? entity.CreatedBy;
            db.IMAGE = entity.ImageUrl;
            await _context.SaveChangesAsync();
            return _mapper.Map<ProductionIssueLog>(db);
        }
    }
}