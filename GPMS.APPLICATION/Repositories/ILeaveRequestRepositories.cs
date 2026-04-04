using GPMS.DOMAIN.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Repositories
{
    public interface ILeaveRequestRepositories
    {
        Task<IEnumerable<LeaveRequest>> GetAllLeaveRequests();
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByUserId(int userId);
        Task<LeaveRequest> GetLeaveRequestById(int id);
        Task<LeaveRequest> CreateLeaveRequest(int userId, string content, DateTime fromDate, DateTime toDate);
        Task<LeaveRequest> DenyLeaveRequest(int id, string denyContent, int approverId);
        Task<LeaveRequest> ApproveLeaveRequest(int id, int approverId);
        Task<LeaveRequest> CancelLeaveRequest(int id, int userId);
    }
}