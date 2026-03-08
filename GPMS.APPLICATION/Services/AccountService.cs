using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public AccountService(IBaseAccountRepositories accountBaseRepo, IBaseRepositories<Role> roleBaseRepo)
        {
            _accountBaseRepo = accountBaseRepo ?? throw new ArgumentNullException(nameof(accountBaseRepo));
            _roleBaseRepo = roleBaseRepo ?? throw new ArgumentNullException(nameof(roleBaseRepo));
        }

        public async Task<RegisterDTO> Register(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            RegisterDTO registerDTO = new RegisterDTO() { User = user, Status = Enum.RegisterStatus.Creating };
            try
            {
                if (!ValidateUserName(user.UserName))
                {
                    ValidationField.AddFieldError(registerDTO.Errors, "UserName", "Tên đăng nhập phải từ 6 đến 50 ký tự");
                    registerDTO.Status = Enum.RegisterStatus.Failed;
                }

                if (!ValidatePasswordLength(user.PasswordHash))
                {
                    ValidationField.AddFieldError(registerDTO.Errors, "Password", "Mật khảu phải từ 6 đến 50 ký tự");
                    registerDTO.Status = Enum.RegisterStatus.Failed;
                }

                if (Enum.RegisterStatus.Creating == registerDTO.Status)
                {
                    var hashedPassword = new PasswordHasher<User>().HashPassword(user, user.PasswordHash);
                    user.PasswordHash = hashedPassword;
                    // Gán role mặc định cho user mới đăng ký (ví dụ: "Customer")s
                    var defaultRole = await _roleBaseRepo.GetAll(null);

                    // Đăng ký tài khoản mới cho customer
                    await _accountBaseRepo.Register(user);
                    
                    
                    registerDTO.Status = Enum.RegisterStatus.Success;
                }
            }
            catch (DbUpdateException ex)
            {
                ValidationField.AddFieldError(registerDTO.Errors, "Exception", ex.Message);
                registerDTO.Status = Enum.RegisterStatus.Failed;
            }
            return registerDTO;
        }

        private bool ValidateUserName(string username)
        {
            if (username.Length >= 6 && username.Length <= 50) return true;
            return false;
        }

        private bool ValidatePasswordLength(string password)
        {
            if (password.Length >= 6 && password.Length <= 50) return true;
            return false;
        }

        public async Task<LoginDTO> Login(string username, string password)
        {
            var user = await _accountBaseRepo.FindUserByUserName(username);
            if (user is null) return null;
            // Check hashed password 
            var hashedPassword = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password);
            if (hashedPassword is PasswordVerificationResult.Failed)
            {
                return null;
            }
            var userRole = await _roleBaseRepo.GetAll(user);
            LoginDTO data = new LoginDTO() { User = user, UserRole = userRole };
            return data;
        }


    }
}
