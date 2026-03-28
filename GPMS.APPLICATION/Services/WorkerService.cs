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
        private readonly IBaseRepositories<Role> _roleRepo;
        private readonly IBaseRepositories<UserStatus> _userStatusRepo;
        private readonly IBaseWorkerRepository _workerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public WorkerService( IBaseRepositories<Role> roleRepo, IBaseRepositories<UserStatus> userStatusRepo, IBaseWorkerRepository workerRepo, IUnitOfWork unitOfWork)
        {
            _roleRepo = roleRepo;
            _userStatusRepo = userStatusRepo ?? throw new ArgumentNullException(nameof(userStatusRepo));
            _workerRepo = workerRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllEmployees()
        {
            var data = await _workerRepo.GetAll(null);
            return data;
        }

        public async Task<User> GetEmployeeById(int id)
        {
            var data = await _workerRepo.GetWorkerById(id);
            return data;
        }

        public async Task<User> CreateEmployee(User user)
        {
            if (user == null)
                throw new Exception("Failed to create worker.");

            var status = await _userStatusRepo.GetById(user.StatusId);
            if (status == null)
                throw new KeyNotFoundException($"Status with Id '{user.StatusId}' not found.");
            User createdUser = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (user.Roles != null && user.Roles.Any())
                {
                    foreach (var role in user.Roles)
                    {
                        var existingRole = await _roleRepo.GetById(role.Id);

                        if (existingRole == null)
                            throw new KeyNotFoundException($"Role with Id '{role.Id}' not found.");
                    }
                }
                if (user.Roles.Any(r => r.Id == 4))
                {
                    user.ManagerId = 1;

                    var existOwner = await _workerRepo.GetWorkerById(user.ManagerId);
                    if (existOwner == null)
                        throw new KeyNotFoundException($"Không tìm thấy Owner với Id '{user.ManagerId}'.");
                }

                if (user.Roles.Any(r => r.Id == 5))
                {
                    if (user.ManagerId == null)
                        throw new KeyNotFoundException("Worker phải có ManagerId.");

                    var existManager = await _workerRepo.GetWorkerById(user.ManagerId);
                    if (existManager == null)
                        throw new KeyNotFoundException($"Không tìm thấy quản lý '{user.ManagerId}'.");

                    var isPM = existManager.Roles != null && existManager.Roles.Any(r => r.Id == 4);
                    if (!isPM)
                        throw new KeyNotFoundException("Manager được chọn không phải là PM.");
                }

                createdUser = await _workerRepo.Create(user);

                await _unitOfWork.SaveChangesAsync();
            });

            return createdUser;
        }

        public async Task<User> UpdateEmployee(int userId, User user)
        {
            if (user == null)
                throw new Exception("Failed to update employee.");
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
            User updateUser = null;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (user.Roles.Any(r => r.Id == 4))
            {
                user.ManagerId = 1;

                var existOwner = await _workerRepo.GetWorkerById(user.ManagerId);
                if (existOwner == null)
                    throw new KeyNotFoundException($"Không tìm thấy Owner với Id '{user.ManagerId}'.");
            }

            if (user.Roles.Any(r => r.Id == 5))
            {
                if (user.ManagerId == null)
                    throw new KeyNotFoundException("Worker phải có ManagerId.");

                var existManager = await _workerRepo.GetWorkerById(user.ManagerId);
                if (existManager == null)
                    throw new KeyNotFoundException($"Không tìm thấy quản lý '{user.ManagerId}'.");

                var isPM = existManager.Roles != null && existManager.Roles.Any(r => r.Id == 4);
                if (!isPM)
                    throw new KeyNotFoundException("Manager được chọn không phải là PM.");
            }
                updateUser = await _workerRepo.Update(user);
                await _unitOfWork.SaveChangesAsync();
            });

            return updateUser;
        }

        public async Task<IEnumerable<User>> GetAllEmployeesByPMId(int id)
        {
            var data = await _workerRepo.GetAll(id);
            return data;
        }
    }
}
