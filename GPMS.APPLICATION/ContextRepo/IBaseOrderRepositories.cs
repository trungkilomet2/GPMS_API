using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseOrderRepositories : IBaseRepositories<Order>
    {
        Task<Order> UpdateOrder(int orderId, Order updatedOrder, List<OHistoryUpdate> histories);
        Task<Order> ChangeStatus (int orderId, int newStatus);
    }
}