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
        IEnumerable<PermissionEntry> GetAll();
        Dictionary<string, string> GetRoleMap();
    }
}
