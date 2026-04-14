using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class PermissionEntry
    {
        public int Id { get; set; }
        public string Controller { get; set; } = null!;
        public string Method { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string RoleIds { get; set; } = string.Empty;
    }
}
