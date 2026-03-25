using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class LogEventService : ILogEventRepositories
    {
        public readonly IBaseRepositories<LogEvent> _logEventRepositories;

        public LogEventService(IBaseRepositories<LogEvent> logEventRepositories)
        {
            _logEventRepositories = logEventRepositories ?? throw new ArgumentNullException(nameof(logEventRepositories));
        }

        public async Task<IEnumerable<LogEvent>> GetAllLog()
        {
            var data = await _logEventRepositories.GetAll(null);
            return data;
        }
    }
}
