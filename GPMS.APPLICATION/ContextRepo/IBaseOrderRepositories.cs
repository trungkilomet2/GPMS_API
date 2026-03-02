using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseOrderRepositories
    {
        Task<IEnumerable<Order>> GetAllOrders();
    }
}