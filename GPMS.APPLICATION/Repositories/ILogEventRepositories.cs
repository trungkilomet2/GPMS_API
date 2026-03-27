using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface ILogEventRepositories
    {
        Task<IEnumerable<LogEvent>> GetAllLog();
        Task<IEnumerable<LogEvent>> GetPermissionAuditLogs(DateTime? from, DateTime? to);
    }
}
