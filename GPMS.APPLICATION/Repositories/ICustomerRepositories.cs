using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface ICustomerRepositories
    {
        Task<IEnumerable<User>> GetAllCustomer();
    }
}
