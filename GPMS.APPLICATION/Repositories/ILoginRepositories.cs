using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IAccountRepositories
    {
        Task<User> Login(string username, string password);

        Task<User> Register(User user);

    }
}
