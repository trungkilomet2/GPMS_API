using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseUserRoleRepo
    {
        Task AddUserRole(string roleName);
        Task RemoveUserRole(string roleName);
    }
}
