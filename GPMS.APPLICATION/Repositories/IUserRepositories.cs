using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IUserRepositories
    {   
       Task<IEnumerable<User>> GetAllUser();
        Task<User> GetOwner();
        Task<User> GetUserById(int id);
        Task<bool> IsEmailExists(string email);
        Task<User> CreateNewUser(User user, List<int> roleIds);
       Task DisableAnUser(int userId);
       Task AssignRoles(int userId, List<int> roleIds);
       Task<User> ViewProfile(int id);
       Task<User> UpdateProfile(int userId, User user);
        Task<User> UpdateUserForAdmin(int userId,User user);
    }
}
