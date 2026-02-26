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
        private readonly IBaseRepository<User> _userBaseRepo;   

        public UserService(IBaseRepository<User> userBaseRepo)
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
            var data = await _userBaseRepo.GetAll();
            
            return data;
        }

        public Task<User> Login(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
