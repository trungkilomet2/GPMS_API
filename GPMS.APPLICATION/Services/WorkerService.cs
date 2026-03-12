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
        private readonly IBaseWorkerRepositories _workerRepo;

        public WorkerService(IBaseWorkerRepositories workerRepo)
        {
            _workerRepo = workerRepo ?? throw new ArgumentNullException(nameof(workerRepo));
        }

        public async Task<IEnumerable<User>> GetAllEmployees()
        {
            var data = await _workerRepo.GetEmployees();
            return data;
        }

        public async Task<User> GetEmployeeById(int id)
        {
            var data = await _workerRepo.GetEmployeeById(id);
            return data;
        }
    }
}
