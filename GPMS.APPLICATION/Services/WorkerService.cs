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
    public class WorkerService : IWorkerRepositories
    {
        private readonly IBaseRepositories<User> _workerRepo;

        public WorkerService(IBaseRepositories<User> workerRepo)
        {
            _workerRepo = workerRepo ?? throw new ArgumentNullException(nameof(workerRepo));
        }

        public async Task<IEnumerable<User>> GetAllEmployees()
        {
            var data = await _workerRepo.GetAll(null);
            return data;
        }

        public async Task<User> GetEmployeeById(int id)
        {
            var data = await _workerRepo.GetById(id);
            return data;
        }
    }
}
