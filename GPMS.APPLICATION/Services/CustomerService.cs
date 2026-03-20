using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class CustomerService : ICustomerRepositories
    {
        private readonly IBaseCustomerRepositories _customerRepositories;

        public CustomerService(IBaseCustomerRepositories customerRepositories)
        {
            _customerRepositories = customerRepositories ?? throw new ArgumentNullException(nameof(customerRepositories));
        }

        public async Task<IEnumerable<User>> GetAllCustomer()
        {
            var data = await _customerRepositories.GetAllCustomer();
            return data;
        }
    }
}
