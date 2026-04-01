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
        private readonly IBaseRepositories<WorkerSkill> _workerRoleRepo;
        private readonly IBaseWorkerRoleRepositories _baseWorkerRepo;

        public WorkerRoleService(IBaseRepositories<WorkerSkill> workerRoleRepo, IBaseWorkerRoleRepositories baseWorkerRepo)
        {
            _workerRoleRepo = workerRoleRepo ?? throw new ArgumentNullException(nameof(workerRoleRepo));
            _baseWorkerRepo = baseWorkerRepo ?? throw new ArgumentNullException(nameof(baseWorkerRepo));
        }

        public async Task<WorkerSkill> CreateWorkerRole(WorkerSkill workerRole)
        {
            var result = await _baseWorkerRepo.FindRoleByName(workerRole.Name);
            if (result != null)
            {
                throw new Exception("Worker role with the same name already exists.");
            }
            var data = await _workerRoleRepo.Create(workerRole);
            return data;
        }

        public async Task<IEnumerable<WorkerSkill>> GetAllWorkerRoles()
        {
            var data = await _workerRoleRepo.GetAll(null);
            return data;
        }
    }
}
