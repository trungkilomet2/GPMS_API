using GPMS.DOMAIN.Entities;

namespace GPMS.APPLICATION.Repositories
{
    public interface IPermissionRepositories
    {
        IEnumerable<PermissionEntry> GetAll();
        Dictionary<string, string> GetRoleMap();
    }
}
