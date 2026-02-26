using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class LoginService : IAccountRepositories
    {   
        private readonly IBaseRepository<User> _userBaseRepo;

        public LoginService(IBaseRepository<User> userBaseRepo)
        {
            _userBaseRepo = userBaseRepo ?? throw new ArgumentNullException(nameof(userBaseRepo));
        }

        public async Task<User> Login(string username, string password)   
        {
            LoginDTO account = new LoginDTO() { UserName = username, Password = password };
            
            var data = await _userBaseRepo.GetAll();

              data.Where(u => u.Username == account.UserName && u.PasswordHash == account.Password);
            if (data.Count() > 2)
            {
               // Error
            }

            return data.FirstOrDefault()!;
        }


        public Task<User> Register(User user)
        {
            throw new NotImplementedException();
        }

    }
}
