using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseAccountRepositories
    {
        Task<User> Login(string username, string password);
        Task<User> Register(User user);    

    }
}
