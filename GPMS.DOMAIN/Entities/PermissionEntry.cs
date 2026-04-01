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
        public string Controller { get; set; }
        public string Method { get; set; }
        public string Action { get; set; }
        public string RoleIds { get; set; }

        public PermissionEntry(int id, string controller, string method, string action, string roleIds)
        {
            Id = id;
            Controller = controller;
            Method = method;
            Action = action;
            RoleIds = roleIds;
        }
    }
}
