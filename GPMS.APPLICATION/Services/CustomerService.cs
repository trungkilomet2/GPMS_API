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
        private readonly IBaseRepositories<User> _userService;

        public CustomerService(IBaseCustomerRepositories customerRepositories, IBaseRepositories<User> userService)
        {
            _customerRepositories = customerRepositories ?? throw new ArgumentNullException(nameof(customerRepositories));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<IEnumerable<User>> GetAllCustomer()
        {
            var data = await _customerRepositories.GetAllCustomer();
            return data;
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerId(int customerId)
        {
            var existingCustomer = await _userService.GetById(customerId);
            if (existingCustomer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customerId} does not exist.");
            }
            var data = await _customerRepositories.GetOrdersByCustomerId(customerId);
            if(data == null)
            {
                throw new Exception($"No orders found for customer with ID {customerId}");
            }
            return data;
        }
    }
}
