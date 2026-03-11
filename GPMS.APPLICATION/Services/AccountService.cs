using GPMS.APPLICATION.Common;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using GPMS.DOMAIN.Enums;
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
        private readonly IBaseUserRoleRepo _userRoleRepo;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IBaseAccountRepositories accountBaseRepo, IBaseRepositories<Role> roleBaseRepo, IBaseUserRoleRepo userRoleRepo, IUnitOfWork unitOfWork)
        {
            _accountBaseRepo = accountBaseRepo ?? throw new ArgumentNullException(nameof(accountBaseRepo));
            _roleBaseRepo = roleBaseRepo ?? throw new ArgumentNullException(nameof(roleBaseRepo));
            _userRoleRepo = userRoleRepo;
            _unitOfWork = unitOfWork;
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
                    await _unitOfWork.ExecuteInTransactionAsync(async () =>
                    {
                        await _accountBaseRepo.Register(user);
                        await _unitOfWork.SaveChangesAsync();
                        User createdUser = await _accountBaseRepo.FindUserByUserName(user.UserName);
                        if (createdUser is null)
                        {
                            throw new Exception("Không tìm thấy tài khoản vừa tạo");
                        }
                        await _userRoleRepo.AddUserRole(createdUser, Roles_Constants.Customer);
                        await _unitOfWork.SaveChangesAsync();
                    });
                    registerDTO.Status = Enum.RegisterStatus.Success;
                }
            }
            catch (DbUpdateException ex)
            {
                ValidationField.AddFieldError(registerDTO.Errors, "Exception", ex.Message);
                registerDTO.Status = Enum.RegisterStatus.Failed;
            }
            catch (Exception ex)
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
