using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class UserService : IUserRepositories
    {   
        private readonly IBaseRepositories<User> _userBaseRepo;   

        public UserService(IBaseRepositories<User> userBaseRepo)
        {
            _userBaseRepo = userBaseRepo ?? throw new ArgumentNullException(nameof(userBaseRepo));
        }   

        public Task<User> CreateNewUser(User user)
        {
            throw new NotImplementedException();
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

        public Task<User> Login(string UserName, string password)
        {
            throw new NotImplementedException();
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
    }
}
