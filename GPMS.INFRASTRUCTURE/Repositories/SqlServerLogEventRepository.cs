using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerLogEventRepository : IBaseRepositories<LogEvent>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerLogEventRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<LogEvent> Create(LogEvent entity)
        {
            var logEvent = _mapper.Map<LOG_EVENTS>(entity);
            await _context.LOG_EVENTS.AddAsync(logEvent);
            await _context.SaveChangesAsync();
            return _mapper.Map<LogEvent>(logEvent);
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LogEvent>> GetAll(object? obj)
        {
            throw new NotImplementedException();
        }

        public Task<LogEvent> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<LogEvent> Update(LogEvent entity)
        {
            throw new NotImplementedException();
        }
    }
}
