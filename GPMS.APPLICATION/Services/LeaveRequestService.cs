using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.DOMAIN.Constants;
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

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByUserId(int userId)
            => await _leaveRequestBaseRepo.GetAll(userId);

        private static readonly TimeZoneInfo _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public async Task<LeaveRequest> CreateLeaveRequest(int userId, string content, DateTime? fromDate, DateTime? toDate)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone).Date;

            if (toDate.HasValue && toDate.Value.Date <= today)
                throw new ArgumentException("ToDate must be at least 1 day after the created date.");

            if (fromDate.HasValue && toDate.HasValue && toDate.Value.Date < fromDate.Value.Date)
                throw new ArgumentException("ToDate must be greater than or equal to FromDate.");

            var leaveRequest = new LeaveRequest
            {
                UserId = userId,
                Content = content,
                DateCreate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone),
                FromDate = fromDate,
                ToDate = toDate,
            };

            return await _leaveRequestBaseRepo.Create(leaveRequest);
        }

        public async Task<LeaveRequest> GetLeaveRequestById(int id)
            => await _leaveRequestBaseRepo.GetById(id);

        public async Task<LeaveRequest> DenyLeaveRequest(int id, string denyContent, int approverId)
        {
            var leaveRequest = await _leaveRequestBaseRepo.GetById(id);

            if (leaveRequest is null)
                throw new KeyNotFoundException($"Leave request with id '{id}' not found.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Only leave requests with status '{LeaveRequestStatus_Constants.Pending}' can be denied.");

            leaveRequest.DenyContent = denyContent;
            leaveRequest.StatusId = LeaveRequestStatus_Constants.Denied_ID;
            leaveRequest.ApprovedBy = approverId;
            leaveRequest.DateReply = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            return await _leaveRequestBaseRepo.Update(leaveRequest);
        }

        public async Task<LeaveRequest> ApproveLeaveRequest(int id, int approverId)
        {
            var leaveRequest = await _leaveRequestBaseRepo.GetById(id);

            if (leaveRequest is null)
                throw new KeyNotFoundException($"Leave request with id '{id}' not found.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Only leave requests with status '{LeaveRequestStatus_Constants.Pending}' can be approved.");

            leaveRequest.StatusId = LeaveRequestStatus_Constants.Approved_ID;
            leaveRequest.ApprovedBy = approverId;
            leaveRequest.DateReply = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            return await _leaveRequestBaseRepo.Update(leaveRequest);
        }

        public async Task<LeaveRequest> CancelLeaveRequest(int id, int userId)
        {
            var leaveRequest = await _leaveRequestBaseRepo.GetById(id);

            if (leaveRequest is null)
                throw new KeyNotFoundException($"Leave request with id '{id}' not found.");

            if (leaveRequest.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to cancel this leave request.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Only leave requests with status '{LeaveRequestStatus_Constants.Pending}' can be cancelled.");

            leaveRequest.StatusId = LeaveRequestStatus_Constants.Cancelled_ID;
            leaveRequest.DateReply = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            return await _leaveRequestBaseRepo.Update(leaveRequest);
        }
    }
}