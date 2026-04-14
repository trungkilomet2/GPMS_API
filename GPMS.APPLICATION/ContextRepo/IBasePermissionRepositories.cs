using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBasePermissionRepositories
    {
        Task<IEnumerable<PermissionEntry>> GetAll();
        Task<PermissionEntry?> GetById(int id);
        Task<Dictionary<string, string>> GetRoleMap();
        Task<IEnumerable<Role>> GetAllRoles();
        Task<bool> UpdateRoleAuthorize(int id, string? roleAuthorize);
        Task<PermissionEntry?> GetByEndpoint(string controller, string method, string action);
    }
}
