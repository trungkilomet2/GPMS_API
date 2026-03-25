namespace GMPS.API.DTOs
{
    public class LeaveRequestDetailDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? Content { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? DateReply { get; set; }
        public string? DenyContent { get; set; }
        public string? CancelContent { get; set; }
        public string? RejectCancelContent { get; set; }
        public string? ApprovedByName { get; set; }
        public string? Status { get; set; }
    }
}