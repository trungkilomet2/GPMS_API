using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class UserService : IUserRepositories
    {
        public Task<User> CreateNewUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task DisableAnUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetUser()
        {
            throw new NotImplementedException();
        }

        public Task<User> Login(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
