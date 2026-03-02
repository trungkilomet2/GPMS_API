using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{

    // Customer dang nhap bang so dien thoai va password
    // Nhan vien dang nhap bang username va password
    public interface IBaseAccountRepositories
    {
        Task<User> Login(string UserName, string password);
        Task<User> Register(User user);
        Task<User> FindUserByUserName(string username);

    }
}
