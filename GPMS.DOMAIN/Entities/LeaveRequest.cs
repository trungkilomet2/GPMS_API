using System;

namespace GPMS.DOMAIN.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? Content { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateReply { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DenyContent { get; set; }
        public string? CancelContent { get; set; }
        public string? RejectCancelContent { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}