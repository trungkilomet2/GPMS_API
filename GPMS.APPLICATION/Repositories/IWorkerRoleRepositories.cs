using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IWorkerRoleRepositories
    {
        Task<IEnumerable<WorkerSkill>> GetAllWorkerRoles();
        Task<WorkerSkill> CreateWorkerRole(WorkerSkill workerRole);
    }
}
