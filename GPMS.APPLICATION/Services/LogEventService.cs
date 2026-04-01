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

        public async Task<IEnumerable<LogEvent>> GetPermissionAuditLogs(DateTime? from, DateTime? to)
        {
            var data = await _logEventRepositories.GetAll(null);
            var result = data.Where(x => x.Message != null && x.Message.StartsWith("PERMISSION_AUDIT"));

            if (from.HasValue)
                result = result.Where(x => x.TimeStemp.HasValue && x.TimeStemp.Value >= from.Value);

            if (to.HasValue)
                result = result.Where(x => x.TimeStemp.HasValue && x.TimeStemp.Value <= to.Value);

            return result;
        }
    }
}
