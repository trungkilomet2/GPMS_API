using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IOrderRepositories
    {
        Task<IEnumerable<Order>> GetAllOrders();
        Task<IEnumerable<Order>> GetOrdersByUserId(int userId);
        Task<Order> GetOrderDetail(int orderId);
        Task<Order> CreateOrder(Order order);
        Task<OMaterial> AddMaterial(int orderId, OMaterial material);
        Task<Order> UpdateOrder(int orderId, Order updatedOrder, List<OHistoryUpdate> histories);
        Task<Order> RequestOrderModification(int orderId, Order updatedOrder, List<OHistoryUpdate> histories);

        Task<Order> DenyOrder(int userId,int orderId, Order updatedOrder, List<OHistoryUpdate> histories);
    }
}