using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Identity;
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
        private readonly IBaseRepositories<Role> _roleBaseRepo;

        public AccountService(IBaseAccountRepositories accountBaseRepo,IBaseRepositories<Role> roleBaseRepo)
        {
            _accountBaseRepo = accountBaseRepo ?? throw new ArgumentNullException(nameof(accountBaseRepo));
            _roleBaseRepo  = roleBaseRepo ?? throw new ArgumentNullException(nameof(roleBaseRepo));
        }

        public Task<User> Register(User user)
        {
            throw new NotImplementedException();
        }


        public async Task<LoginDTO> Login(string username, string password)
        {
            var user = await _accountBaseRepo.FindUserByUserName(username);

            if (user is null) return null;
            // Check hashed password 
            var hashedPassword = new PasswordHasher<User>().VerifyHashedPassword(user,user.PasswordHash, password);    
            if(hashedPassword is PasswordVerificationResult.Failed)
            {
                return null;
            }
            var userRole = await _roleBaseRepo.GetAll(user);
            LoginDTO data = new LoginDTO() { User = user, UserRole = userRole };
            return data;

        }



    }
}
