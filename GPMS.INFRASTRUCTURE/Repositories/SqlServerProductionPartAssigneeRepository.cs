using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerProductionPartAssigneeRepository : IBaseProductionPartAssignRepositories
    {
        public Task<ProductionPart> AssignWorkers(int partId, IEnumerable<int> workerIds)
        {
            throw new NotImplementedException();
        }

        public Task<ProductionPartWorkLog> CreateWorkLog(ProductionPartWorkLog entity)
        {
            throw new NotImplementedException();
        }

        public Task<ProductionPartWorkLog> GetWorkLogById(int workLogId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductionPartWorkLog> MarkWorkLogAsPaid(int workLogId)
        {
            throw new NotImplementedException();
        }

        public Task<int> MarkWorkLogsReadOnlyAfterDate(DateOnly date)
        {
            throw new NotImplementedException();
        }

        public Task<ProductionPart> RemoveWorker(int partId, int workerId)
        {
            throw new NotImplementedException();
        }
    }
}
