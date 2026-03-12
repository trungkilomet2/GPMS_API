using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.Services
{
    public class LeaveRequestService : ILeaveRequestRepositories
    {
        private readonly IBaseRepositories<LeaveRequest> _leaveRequestBaseRepo;

        public LeaveRequestService(IBaseRepositories<LeaveRequest> leaveRequestBaseRepo)
        {
            _leaveRequestBaseRepo = leaveRequestBaseRepo ?? throw new ArgumentNullException(nameof(leaveRequestBaseRepo));
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequests()
            => await _leaveRequestBaseRepo.GetAll(null);

        public async Task<LeaveRequest> GetLeaveRequestById(int id)
            => await _leaveRequestBaseRepo.GetById(id);
    }
}