using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Enums
{   

    /// <summary>
    /// RoleName Equal ID in Role Table in database 
    /// </summary>
    public enum RoleName
    {
        None = 0,
        Admin = 1,
        Customer = 2,
        Owner = 3,
        PM = 4,
        Worker = 5
    }
}
