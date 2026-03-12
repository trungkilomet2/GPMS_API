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
        private readonly IBaseRepositories<Role> _roleRepo;
        private readonly IBaseRepositories<UserStatus> _userStatusRepo;

        public WorkerService(IBaseRepositories<User> workerRepo, IBaseRepositories<Role> roleRepo, IBaseRepositories<UserStatus> userStatusRepo)
        {
            _workerRepo = workerRepo ?? throw new ArgumentNullException(nameof(workerRepo));
            _roleRepo = roleRepo;
            _userStatusRepo = userStatusRepo ?? throw new ArgumentNullException(nameof(userStatusRepo));
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

        public async Task<User> CreateEmployee(User user)
        {
            if (user == null)
                throw new Exception("Failed to create worker.");

            var status = await _userStatusRepo.GetById(user.StatusId);
            if (status == null)
                throw new KeyNotFoundException($"Status with Id '{user.StatusId}' not found.");

            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    var existingRole = await _roleRepo.GetById(role.Id);

                    if (existingRole == null)
                        throw new KeyNotFoundException($"Role with Id '{role.Id}' not found.");
                }
            }

            return await _workerRepo.Create(user);
        }

        public async Task<User> UpdateEmployee(int userId, User user)
        {
            if (user == null)
                throw new Exception("Failed to update employee.");
            var status = await _userStatusRepo.GetById(user.StatusId);
            if (status == null)
                throw new KeyNotFoundException($"Status with Id '{user.StatusId}' not found.");

            var existing = await _workerRepo.GetById(userId);
            if (existing == null)
                throw new KeyNotFoundException($"Employee with id '{userId}' not found.");

            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    var existingRole = await _roleRepo.GetById(role.Id);

                    if (existingRole == null)
                        throw new KeyNotFoundException($"Role with Id '{role.Id}' not found.");
                }
            }
            return await _workerRepo.Update(user);
        }
    }
}
