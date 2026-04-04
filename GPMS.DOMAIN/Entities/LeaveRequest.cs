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
        // Trungnt 04-01-26 : Xóa ? của fromdate với todate thành bắt nó phải not null
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? DenyContent { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}