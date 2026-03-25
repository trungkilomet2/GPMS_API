using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class OrderRejectService : IOrderRejectRepositories
    {
        private readonly IBaseRepositories<OrderRejectReason> _orderRejectRepo;
        private readonly IBaseRepositories<Order> _orderRepo;
        private readonly IBaseOrderRepositories _baseOrderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderRejectService(IBaseRepositories<OrderRejectReason> orderRejectRepo, IUnitOfWork unitOfWork, IBaseRepositories<Order> orderRepo, IBaseOrderRepositories baseOrderRepo)
        {
            _orderRejectRepo = orderRejectRepo ?? throw new ArgumentNullException(nameof(orderRejectRepo));
            _unitOfWork = unitOfWork;
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _baseOrderRepo = baseOrderRepo ?? throw new ArgumentNullException(nameof(baseOrderRepo));
        }

        public async Task<OrderRejectReason> CreateReason(OrderRejectReason entity)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingOrder = await _orderRepo.GetById(entity.OrderId);
                if (existingOrder == null)
                {
                    throw new Exception($"Order with ID {entity.OrderId} does not exist.");
                }
                if(existingOrder.StatusName != OrderStatus_Constants.Pending)
                {
                    throw new Exception($"Order with ID {entity.OrderId} is not in a pending state and cannot be rejected.");
                }
                await _baseOrderRepo.ChangeStatus(entity.OrderId, 4);
                await _unitOfWork.SaveChangesAsync();
                await _orderRejectRepo.Create(entity);
                await _unitOfWork.SaveChangesAsync();
            });
            return entity;
        }

        public async Task<OrderRejectReason> GetReasonById(int id)
        {
            if(id <= 0)
            {
                throw new Exception("Invalid ID. ID must be greater than zero.");
            }
            var existingOrder = await _orderRepo.GetById(id);
            if (existingOrder == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} does not exist.");
            }
            var reason = await _orderRejectRepo.GetById(id);
            return reason;
        }
    }
}
