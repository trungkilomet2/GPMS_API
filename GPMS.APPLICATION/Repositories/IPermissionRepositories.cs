using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface IPermissionRepositories
    {
        Task<IEnumerable<PermissionEntry>> GetAll();
        Task<PermissionEntry?> GetById(int id);
        Task<Dictionary<string, string>> GetRoleMap();
        Task<IEnumerable<Role>> GetAllRoles();
        Task<bool> UpdateRoleAuthorize(int id, string? roleAuthorize);
    }
}
