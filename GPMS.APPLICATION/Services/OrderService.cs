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
        private readonly IBaseRepositories<Order> _orderBaseRepo;

        public OrderService(IBaseRepositories<Order> orderBaseRepo)
        {
            _orderBaseRepo = orderBaseRepo ?? throw new ArgumentNullException(nameof(orderBaseRepo));
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
            => await _orderBaseRepo.GetAll(null);

        public async Task<IEnumerable<Order>> GetOrdersByUserId(int userId)
            => await _orderBaseRepo.GetAll(userId);

        public async Task<Order> GetOrderDetail(int orderId)
            => await _orderBaseRepo.GetById(orderId);
    }
}