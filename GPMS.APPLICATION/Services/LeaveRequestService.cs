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

        public async Task<LeaveRequest> CreateLeaveRequest(int userId, string content, DateTime fromDate, DateTime toDate)
        {
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone).Date;

            if (fromDate.Date < today)
                throw new InvalidOperationException("Ngày bắt đầu không được để quá khứ.");

            if (toDate.Date < today)
                throw new InvalidOperationException("Ngày kết thúc không được để quá khứ.");

            if (toDate.Date <= fromDate.Date)
                throw new InvalidOperationException("Ngày kết thúc không được trước ngày bắt đầu.");

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
                throw new KeyNotFoundException($"Không tìm thấy yêu cầu nghỉ phép với id '{id}'.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Chỉ có thể từ chối yêu cầu nghỉ phép ở trạng thái '{LeaveRequestStatus_Constants.Pending}'.");

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
                throw new KeyNotFoundException($"Không tìm thấy yêu cầu nghỉ phép với mã '{id}'.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Chỉ có thể phê duyệt yêu cầu nghỉ phép ở trạng thái '{LeaveRequestStatus_Constants.Pending}'.");

            leaveRequest.StatusId = LeaveRequestStatus_Constants.Approved_ID;
            leaveRequest.ApprovedBy = approverId;
            leaveRequest.DateReply = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            return await _leaveRequestBaseRepo.Update(leaveRequest);
        }

        public async Task<LeaveRequest> CancelLeaveRequest(int id, int userId)
        {
            var leaveRequest = await _leaveRequestBaseRepo.GetById(id);

            if (leaveRequest is null)
                throw new KeyNotFoundException($"Không tìm thấy yêu cầu nghỉ phép với id '{id}'.");

            if (leaveRequest.UserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền hủy yêu cầu nghỉ phép này.");

            if (leaveRequest.StatusName != LeaveRequestStatus_Constants.Pending)
                throw new InvalidOperationException($"Chỉ có thể hủy yêu cầu nghỉ phép ở trạng thái '{LeaveRequestStatus_Constants.Pending}'.");

            leaveRequest.StatusId = LeaveRequestStatus_Constants.Cancelled_ID;
            leaveRequest.DateReply = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

            return await _leaveRequestBaseRepo.Update(leaveRequest);
        }
    }
}