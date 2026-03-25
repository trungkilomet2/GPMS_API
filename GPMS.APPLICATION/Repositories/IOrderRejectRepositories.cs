using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface IOrderRejectRepositories
    {
        Task<OrderRejectReason> CreateReason(OrderRejectReason entity);
        Task<OrderRejectReason> GetReasonById(int id);
    }
}
