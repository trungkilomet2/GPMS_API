using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerCuttingNotebookRepository : IBaseRepositories<CuttingNotebook>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;
        public SqlServerCuttingNotebookRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CuttingNotebook> Create(CuttingNotebook entity)
        {
            var db = _mapper.Map<CUTTING_NOTEBOOK>(entity);
            await _context.CUTTING_NOTEBOOK.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<CuttingNotebook>(db);
        }

        public Task Delete(object id) => throw new NotImplementedException();

        public async Task<IEnumerable<CuttingNotebook>> GetAll(object? obj)
        {
            IQueryable<CUTTING_NOTEBOOK> query = _context.CUTTING_NOTEBOOK;
            if (obj is int productionId)
            {
                query = query.Where(x => x.PRODUCTION_ID == productionId);
            }
            return _mapper.Map<IEnumerable<CuttingNotebook>>(await query.OrderByDescending(x => x.CP_ID).ToListAsync());
        }

        public async Task<CuttingNotebook> GetById(object id)
        {
            if (id is not int notebookId) return null;
            var data = await _context.CUTTING_NOTEBOOK.FirstOrDefaultAsync(x => x.CP_ID == notebookId);
            return data is null ? null : _mapper.Map<CuttingNotebook>(data);
        }

        public async Task<CuttingNotebook> Update(CuttingNotebook entity)
        {
            var db = await _context.CUTTING_NOTEBOOK.FirstOrDefaultAsync(x => x.CP_ID == entity.Id);
            if (db is null) throw new KeyNotFoundException("Notebook not found");
            db.MARKER_LENGTH = entity.MarkerLength;
            db.FABRIC_WIDTH = entity.FabricWidth;
            await _context.SaveChangesAsync();
            return _mapper.Map<CuttingNotebook>(db);
        }
    }
}
