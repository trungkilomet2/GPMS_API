using GPMS.APPLICATION.DTOs;
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
        Task<LoginDTO> Login(string UserName, string password);
        Task<RegisterDTO> Register(User user);
        Task ChangePassword(int userId, string currentPassword, string newPassword);
    }
}
