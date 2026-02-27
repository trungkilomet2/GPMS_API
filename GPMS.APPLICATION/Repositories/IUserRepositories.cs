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
       Task<IEnumerable<User>> GetAllUser();
       Task<User> CreateNewUser(User user);
       Task DisableAnUser(User user);
       Task<User> ViewProfile(int id);
    }
}
