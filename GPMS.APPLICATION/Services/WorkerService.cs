using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
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
        private readonly IBaseAccountRepositories _accRepo;
        private readonly IBaseWorkerRepository _workerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public WorkerService( IBaseRepositories<Role> roleRepo, IBaseRepositories<UserStatus> userStatusRepo, IBaseWorkerRepository workerRepo, IUnitOfWork unitOfWork, IBaseAccountRepositories accRepo)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userStatusRepo = userStatusRepo ?? throw new ArgumentNullException(nameof(userStatusRepo));
            _workerRepo = workerRepo ?? throw new ArgumentNullException(nameof(workerRepo));
            _unitOfWork = unitOfWork;
            _accRepo = accRepo ?? throw new ArgumentNullException(nameof(accRepo));
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
                throw new Exception("Lỗi khi tạo nhân viên.");

            var status = await _userStatusRepo.GetById(user.StatusId);
            if (status == null)
                throw new KeyNotFoundException($"Trạng thái với Id: '{user.StatusId}' không tồn tại.");
            var existManager = await _workerRepo.GetWorkerById(user.ManagerId);
            if (existManager == null)
                throw new KeyNotFoundException($"Người dùng với Id: '{user.ManagerId}' không tồn tại.");
            User createdUser = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (user.Roles != null && user.Roles.Any())
                {
                    foreach (var role in user.Roles)
                    {
                        var existingRole = await _roleRepo.GetById(role.Id);

                        if (existingRole == null)
                            throw new KeyNotFoundException($"Role với Id '{role.Id}' không tồn tại.");
                    }
                }

                createdUser = await _workerRepo.Create(user);

                await _unitOfWork.SaveChangesAsync();
            });

            return createdUser;
        }

        public async Task<User> UpdateEmployee(int userId, User user)
        {
            if (user == null)
                throw new Exception("Lỗi khi cập nhật nhân viên.");
            var status = await _userStatusRepo.GetById(user.StatusId);
            if (status == null)
                throw new KeyNotFoundException($"Trạng thái với Id: '{user.StatusId}' không tồn tại.");
            var existManager = await _workerRepo.GetWorkerById(user.ManagerId);
            if (existManager == null)
                throw new KeyNotFoundException($"Người dùng với Id: '{user.ManagerId}' không tồn tại.");
            var existing = await _workerRepo.GetWorkerById(userId);
            if (existing == null)
                throw new KeyNotFoundException($"nhân viên với Id: '{userId}' không tồn tại.");

            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    var existingRole = await _roleRepo.GetById(role.Id);

                    if (existingRole == null)
                        throw new KeyNotFoundException($"Role với Id '{role.Id}' không tồn tại.");
                }
            }
            return await _workerRepo.Update(user);
        }

        public async Task<IEnumerable<User>> GetAllEmployeesByPMId(int id)
        {
            var data = await _workerRepo.GetAll(id);
            return data;
        }
    }
}
