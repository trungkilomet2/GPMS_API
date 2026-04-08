using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class UserService : IUserRepositories
    {
        private readonly IBaseRepositories<User> _userBaseRepo;
        private readonly IBaseRepositories<Role> _roleRepo;
        private readonly IBaseUserRoleRepo _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseAccountRepositories _accRepo;

        public UserService(
            IBaseRepositories<User> userBaseRepo,
            IBaseRepositories<Role> roleRepo,
            IBaseUserRoleRepo userRoleRepo,
            IUnitOfWork unitOfWork,
            IBaseAccountRepositories accRepo)
        {
            _userBaseRepo = userBaseRepo ?? throw new ArgumentNullException(nameof(userBaseRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _accRepo = accRepo;
        }

        public async Task<User> CreateNewUser(User user, List<int> roleIds)
        {
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashedPassword;

            User createdUser = null;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                createdUser = await _userBaseRepo.Create(user);
                foreach (var roleId in roleIds)
                {
                    var role = await _roleRepo.GetById(roleId);
                    if (role == null)
                        throw new KeyNotFoundException($"Không tìm thấy vai trò với ID {roleId}.");
                    await _userRoleRepo.AddUserRole(createdUser, role.Name);
                }
            });
            return createdUser;
        }

        public async Task<bool> DisableAnUser(int userId)
        {
            var user = await _userBaseRepo.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {userId}.");

            user.StatusId = user.StatusId == UserStatus_Constants.Active
                ? UserStatus_Constants.Inactive
                : UserStatus_Constants.Active;

            await _userBaseRepo.Update(user);
            return user.StatusId == UserStatus_Constants.Active;
        }

        public async Task AssignRoles(int userId, List<int> roleIds)
        {
            var user = await _userBaseRepo.GetById(userId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {userId}.");
            if (user.StatusId == UserStatus_Constants.Inactive)
                throw new InvalidOperationException("Không thể phân quyền cho người dùng đã bị vô hiệu hóa.");

            var roleNames = new List<string>();
            foreach (var roleId in roleIds)
            {
                var role = await _roleRepo.GetById(roleId);
                if (role == null)
                    throw new KeyNotFoundException($"Không tìm thấy vai trò với ID {roleId}.");
                roleNames.Add(role.Name);
            }

            await _userRoleRepo.ReplaceUserRoles(user, roleNames);
        }

        public async Task<IEnumerable<User>> GetAllUser()
        {
            var data = await _userBaseRepo.GetAll(null);
            return data;
        }

        public async Task<User> ViewProfile(int id)
        {
            var data = await _userBaseRepo.GetById(id);
            if(data == null)
            {
                throw new KeyNotFoundException("Không tìm thấy người dùng");
            }
            return data;
        }

        public async Task<User> UpdateProfile(int userId, User user)
        {
            var result = await _userBaseRepo.GetById(userId);
            if (result == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }
            if (userId != result.Id)
            {
                throw new Exception("Chỉ có thể cập nhật chính bản thân");
            }
            var data = await _userBaseRepo.Update(user);
            return data;
        }

        public async Task<User> GetUserById(int? id)
        {
            var data = await _userBaseRepo.GetById(id);
            if(data == null)
                {
                throw new KeyNotFoundException("Không tìm thấy người dùng");
            }
            return data;
        }

        public async Task<User> UpdateUserForAdmin(int userId, User user)
        {
            var result = await _userBaseRepo.GetById(userId);
            if (result == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng");
            if (result.StatusId == UserStatus_Constants.Inactive)
                throw new InvalidOperationException("Không thể cập nhật người dùng đã bị vô hiệu hóa.");
            user.StatusId = result.StatusId;
            var data = await _userBaseRepo.Update(user);
            return data;
        }

        public Task<User> GetOwner()
        {
            var data = _accRepo.GetOwner();
            if (data == null)
            {
                throw new KeyNotFoundException("Không tìm thấy chủ xưởng");
            }
            return data;
        }

        public async Task<bool> IsEmailExists(string email)
        {
            var data = await _accRepo.GetUserByMail(email.ToLower());
            return data != null;
        }
    }
}
