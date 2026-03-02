using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class OrderService : IOrderRepositories
    {
        private readonly IBaseOrderRepositories _orderBaseRepo;

        public OrderService(IBaseOrderRepositories orderBaseRepo)
        {
            _orderBaseRepo = orderBaseRepo ?? throw new ArgumentNullException(nameof(orderBaseRepo));
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orderBaseRepo.GetAllOrders();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _orderBaseRepo.GetOrderById(id);
        }
    }
}