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

        public async Task<RegisterDTO> Register(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            RegisterDTO registerDTO = new RegisterDTO() { User = user };
            // Hash password before save to database, using Microsoft.AspNetCore.Identity package for hashing password
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashedPassword;
            if (!ValidateUserName(user.UserName))
            {
                registerDTO.Errors.Add("Tên người dùng phải ít nhất 6 ký tự và nhiều nhất 30 kí tự");
                registerDTO.Status = Enum.RegisterStatus.Failed;
            }
            await _accountBaseRepo.Register(user);  
            return registerDTO;
        }

        public bool ValidateUserName(string username)
        {
            if (username.Length > 6 && username.Length < 30) return true;
            return false;
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
