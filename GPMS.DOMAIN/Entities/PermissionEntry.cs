using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class PermissionEntry
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string RoleIds { get; set; }

        public PermissionEntry(string controller, string action, string roleIds)
        {
            Controller = controller;
            Action = action;
            RoleIds = roleIds;
        }
    }
}
