using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerCuttingNotebookLogRepository : IBaseRepositories<CuttingNotebookLog>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerCuttingNotebookLogRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CuttingNotebookLog> Create(CuttingNotebookLog entity)
        {
            var db = _mapper.Map<CUTTING_NOTEBOOK_LOG>(entity);
            await _context.CUTTING_NOTEBOOK_LOG.AddAsync(db);
            await _context.SaveChangesAsync();
            return _mapper.Map<CuttingNotebookLog>(db);
        }

        public async Task Delete(object id)
        {
            if (id is not int logId) return;
            var db = await _context.CUTTING_NOTEBOOK_LOG.FirstOrDefaultAsync(x => x.CND_ID == logId);
            if (db is null) return;
            _context.CUTTING_NOTEBOOK_LOG.Remove(db);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CuttingNotebookLog>> GetAll(object? obj)
        {
            IQueryable<CUTTING_NOTEBOOK_LOG> query = _context.CUTTING_NOTEBOOK_LOG;
            if (obj is int notebookId)
            {
                query = query.Where(x => x.CP_ID == notebookId);
            }
            var data = await query.OrderByDescending(x => x.CND_ID).ToListAsync();
            return _mapper.Map<IEnumerable<CuttingNotebookLog>>(data);
        }

        public async Task<CuttingNotebookLog> GetById(object id)
        {
            if (id is not int logId) return null;
            var data = await _context.CUTTING_NOTEBOOK_LOG.FirstOrDefaultAsync(x => x.CND_ID == logId);
            return data is null ? null : _mapper.Map<CuttingNotebookLog>(data);
        }

        public async Task<CuttingNotebookLog> Update(CuttingNotebookLog entity)
        {
            var db = await _context.CUTTING_NOTEBOOK_LOG.FirstOrDefaultAsync(x => x.CND_ID == entity.Id);
            if (db is null) throw new KeyNotFoundException("Cutting notebook log not found");
            db.COLOR = entity.Color;
            db.METER_PER_KG = entity.MeterPerKg;
            db.LAYER = entity.Layer;
            db.PRODUCT_QTY = entity.ProductQty;
            db.AVG_CONSUMPTION = entity.AvgConsumption;
            db.NOTE = entity.Note;
            await _context.SaveChangesAsync();
            return _mapper.Map<CuttingNotebookLog>(db);
        } 
    }
}
