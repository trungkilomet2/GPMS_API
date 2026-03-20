using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseProductionPartAssignRepositories
    {   
        // Assign Task To Worker
        Task<ProductionPart> AssignWorkers(int partId, IEnumerable<int> workerIds);
        // Remove Worker From Task  
        Task<ProductionPart> RemoveWorker(int partId, int workerId);

    }
}
