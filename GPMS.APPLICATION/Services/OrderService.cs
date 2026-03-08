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
        private readonly IBaseRepositories<OMaterial> _materialBaseRepo;

        public OrderService(
            IBaseRepositories<Order> orderBaseRepo,
            IBaseRepositories<OMaterial> materialBaseRepo)
        {
            _orderBaseRepo = orderBaseRepo ?? throw new ArgumentNullException(nameof(orderBaseRepo));
            _materialBaseRepo = materialBaseRepo ?? throw new ArgumentNullException(nameof(materialBaseRepo));
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
                throw new Exception("Failed to create order.");
            if (order.UserId == 0)
                throw new Exception("User not found.");
            if (order.EndDate < order.StartDate)
                throw new Exception("End date must be greater than start date.");

            return await _orderBaseRepo.Create(order);
        }

        public async Task<OMaterial> AddMaterial(int orderId, OMaterial material)
        {
            var order = await _orderBaseRepo.GetById(orderId);
            if (order is null)
                throw new Exception($"Order với id '{orderId}' không tồn tại trong hệ thống.");

            if (material.Value <= 0)
                throw new Exception("Quantity must be greater than zero.");

            material.OrderId = orderId;
            return await _materialBaseRepo.Create(material);
        }
    }
}