using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IUserRepositories
    {   
       Task<IEnumerable<User>> GetUser();
       Task<User> Login(string username, string password);
       Task<User> CreateNewUser(User user);
       Task DisableAnUser(User user);
    }
}
