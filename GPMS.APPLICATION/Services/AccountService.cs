using GPMS.APPLICATION.ContextRepo;
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
    public class AccountService : IAccountRepositories
    {   
        private readonly IBaseAccountRepositories _accountBaseRepo;

        public AccountService(IBaseAccountRepositories accountBaseRepo)
        {
            _accountBaseRepo = accountBaseRepo ?? throw new ArgumentNullException(nameof(accountBaseRepo));
        }

        public async Task<User> Login(string username, string password)   
        {
            LoginDTO account = new LoginDTO() { UserName = username, Password = password };
            
            var data = await _accountBaseRepo.Login(account.UserName, account.Password);

           

            return data;
        }


        public Task<User> Register(User user)
        {
            throw new NotImplementedException();
        }

    }
}
