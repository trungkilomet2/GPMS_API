using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface ILeaveRequestRepositories
    {
        Task<IEnumerable<LeaveRequest>> GetAllLeaveRequests();
    }
}