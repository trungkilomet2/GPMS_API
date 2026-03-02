using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IOrderRepositories
    {
        Task<IEnumerable<Order>> GetAllOrders();
    }
}