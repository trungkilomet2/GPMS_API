using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Abstractions
{
    public interface IUserInterface
    {   
       Task<IEnumerable<User>> GetUser();
       Task<User> Login(string username, string password);
    
    
    }
}
