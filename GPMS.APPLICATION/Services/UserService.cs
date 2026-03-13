using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
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

        public UserService(
            IBaseRepositories<User> userBaseRepo,
            IBaseRepositories<Role> roleRepo,
            IBaseUserRoleRepo userRoleRepo,
            IUnitOfWork unitOfWork)
        {
            _userBaseRepo = userBaseRepo ?? throw new ArgumentNullException(nameof(userBaseRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
                        throw new Exception($"Role with ID {roleId} not found.");
                    await _userRoleRepo.AddUserRole(createdUser, role.Name);
                }
            });
            return createdUser;
        }

        public Task DisableAnUser(User user)
        {
            throw new NotImplementedException();
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
                throw new Exception("User not found");
            }
            return data;
        }

        public async Task<User> UpdateProfile(int userId, User user)
        {
            var result = await _userBaseRepo.GetById(userId);
            if (result == null)
            {
                throw new Exception(string.Format("Error: {0}", string.Join(" ", "User not found")));
            }
            if (userId != result.Id)
            {
                throw new Exception(string.Format("Error: {0}", string.Join(" ", "Id missmatch")));
            }
            var data = await _userBaseRepo.Update(user);
            return data;
        }

        public Task<User> GetUserById(int id)
        {
            var data = _userBaseRepo.GetById(id);
            return data;
        }        
    }
}
