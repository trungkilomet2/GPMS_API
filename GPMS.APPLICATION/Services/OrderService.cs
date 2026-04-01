using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.DTOs;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class OrderService : IOrderRepositories
    {
        private readonly IBaseOrderRepositories _orderBaseRepo;
        private readonly IBaseRepositories<OMaterial> _materialBaseRepo;
        private readonly IBaseRepositories<User> _userBaseRepo;
        private readonly IBaseOrderStatusRepositories _orderStatusRepo;

        public OrderService(
            IBaseOrderRepositories orderBaseRepo,
            IBaseRepositories<OMaterial> materialBaseRepo,
            IBaseRepositories<User> userBaseRepo,
            IBaseOrderStatusRepositories orderStatusRepo)
        {
            _orderBaseRepo = orderBaseRepo ?? throw new ArgumentNullException(nameof(orderBaseRepo));
            _materialBaseRepo = materialBaseRepo ?? throw new ArgumentNullException(nameof(materialBaseRepo));
            _userBaseRepo = userBaseRepo ?? throw new ArgumentNullException(nameof(userBaseRepo));
            _orderStatusRepo = orderStatusRepo ?? throw new ArgumentNullException(nameof(orderStatusRepo));
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
            var existing = await _userBaseRepo.GetById(order.UserId);
            if (existing == null)
                throw new Exception("User not found.");
            if (order.EndDate < order.StartDate)
                throw new Exception("End date must be greater than start date.");
            if(order.StartDate < DateOnly.FromDateTime(DateTime.Now))
                throw new Exception("Start date must be greater than current date.");
            return await _orderBaseRepo.Create(order);
        }

        public async Task<Order> UpdateOrder(int orderId, int userId, UpdateOrderInput input)
        {
            if (input.EndDate < input.StartDate)
                throw new ArgumentException("End date must be greater than start date.");
            if (input.StartDate < DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Start date must be greater than current date.");

            var existing = await _orderBaseRepo.GetById(orderId);
            if (existing is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' not exist in system.");
            if (existing.StatusName != OrderStatus_Constants.Modification)
                throw new InvalidOperationException($"Only modify order with status '{OrderStatus_Constants.Modification}'.");
            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this order.");

            var resolvedImage = !string.IsNullOrEmpty(input.Image) ? input.Image : existing.Image;

            var histories = new List<OHistoryUpdate>();
            void TrackChange(string field, string? oldVal, string? newVal)
            {
                if (oldVal != newVal)
                    histories.Add(new OHistoryUpdate
                    {
                        OrderId = orderId,
                        FieldName = field,
                        OldValue = oldVal ?? string.Empty,
                        NewValue = newVal ?? string.Empty
                    });
            }

            TrackChange("OrderName", existing.OrderName, input.OrderName);
            TrackChange("Type", existing.Type, input.Type);
            TrackChange("Size", existing.Size, input.Size);
            TrackChange("Color", existing.Color, input.Color);
            TrackChange("StartDate", existing.StartDate.ToString(), input.StartDate.ToString());
            TrackChange("EndDate", existing.EndDate.ToString(), input.EndDate.ToString());
            TrackChange("Quantity", existing.Quantity.ToString(), input.Quantity.ToString());
            TrackChange("Image", existing.Image, resolvedImage);
            TrackChange("Note", existing.Note, input.Note);

            var updatedOrder = new Order
            {
                Id = orderId,
                UserId = userId,
                OrderName = input.OrderName,
                Type = input.Type,
                Size = input.Size,
                Color = input.Color,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Quantity = input.Quantity,
                Image = resolvedImage,
                Note = input.Note,
                Template = input.Templates,
                Material = input.Materials
            };

            return await _orderBaseRepo.UpdateOrder(orderId, updatedOrder, histories);
        }

        public async Task<OMaterial> AddMaterial(int orderId, OMaterial material)
        {
            var order = await _orderBaseRepo.GetById(orderId);
            if (order is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' not found.");

            if (order.StatusName == OrderStatus_Constants.Approved)
                throw new InvalidOperationException("Cannot add material to an approved order.");

            if (material.Value <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            material.OrderId = orderId;
            return await _materialBaseRepo.Create(material);
        }

        public async Task<Order> RequestOrderModification(int orderId, Order updatedOrder, List<OHistoryUpdate> histories)
        {
            if (updatedOrder == null)
                throw new ArgumentNullException(nameof(updatedOrder), "Failed to update order.");
            var existing = await _orderBaseRepo.GetById(orderId);
            if (existing is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' not exist in system.");
            if (existing.StatusName != OrderStatus_Constants.Pending)
                throw new InvalidOperationException("Only modify order with status Chờ Xét Duyệt.");

            return await _orderStatusRepo.RequestOrderModification(orderId, updatedOrder, histories);
        }

        public async Task<Order> DenyOrder(int userId, int orderId)
        {
            var existing = await _orderBaseRepo.GetById(orderId);
            if (existing is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' not exist in system.");
            if (existing.StatusName != OrderStatus_Constants.Pending)
                throw new InvalidOperationException("Only modify order with status Chờ Xét Duyệt.");
            if (existing.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to deny this order.");

            var histories = new List<OHistoryUpdate>
            {
                new OHistoryUpdate
                {
                    OrderId = orderId,
                    FieldName = "Status",
                    OldValue = existing.StatusName ?? string.Empty,
                    NewValue = OrderStatus_Constants.Modification
                }
            };

            var updatedOrder = new Order
            {
                Id = orderId,
                Status = OrderStatus_Constants.Cancelled_ID
            };

            return await _orderStatusRepo.DenyOrder(orderId, updatedOrder, histories);
        }

        public async Task<Order> ApproveOrder(int orderId)
        {
            var existing = await _orderBaseRepo.GetById(orderId);
            if (existing is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' not exist in system.");
            if (existing.StatusName == OrderStatus_Constants.Approved ||
                existing.StatusName == OrderStatus_Constants.Rejected)
                throw new InvalidOperationException("This order request has already been processed.");
            if (existing.StatusName != OrderStatus_Constants.Pending)
                throw new InvalidOperationException("Only approve order with status Chờ Xét Duyệt.");

            var histories = new List<OHistoryUpdate>
            {
                new OHistoryUpdate
                {
                    OrderId = orderId,
                    FieldName = "Status",
                    OldValue = existing.StatusName ?? string.Empty,
                    NewValue = OrderStatus_Constants.Approved
                }
            };

            var updatedOrder = new Order
            {
                Id = orderId,
                Status = OrderStatus_Constants.Approved_ID
            };

            return await _orderStatusRepo.ApproveOrder(orderId, updatedOrder, histories);
        }
    }
}