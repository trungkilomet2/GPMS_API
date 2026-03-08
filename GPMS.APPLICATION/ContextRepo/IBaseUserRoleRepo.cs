using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseUserRoleRepo
    {
        Task AddUserRole(User user,string roleName);
        Task RemoveUserRole(User user,string roleName);
    }
}
