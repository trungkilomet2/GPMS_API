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
        public async Task<Order> CreateOrder(Order order)
        {
            if (order == null)
            {
                throw new Exception("Failed to create order.");
            }
            if (order.UserId == null)
            {
                throw new Exception("User not found.");
            }
            if(order.EndDate < order.StartDate)
            {
                throw new Exception("End date must be greater than start date.");
            }
            var data = await _orderBaseRepo.Create(order);
            return data;
        }

    }
}