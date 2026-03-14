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
    public class WorkerRoleService : IWorkerRoleRepositories
    {
        private readonly IBaseRepositories<WorkerRole> _workerRoleRepo;

        public WorkerRoleService(IBaseRepositories<WorkerRole> workerRoleRepo)
        {
            _workerRoleRepo = workerRoleRepo ?? throw new ArgumentNullException(nameof(workerRoleRepo));
        }

        public async Task<WorkerRole> CreateWorkerRole(WorkerRole workerRole)
        {
            var data = await _workerRoleRepo.Create(workerRole);
            return data;
        }

        public async Task<IEnumerable<WorkerRole>> GetAllWorkerRoles()
        {
            var data = await _workerRoleRepo.GetAll(null);
            return data;
        }
    }
}
