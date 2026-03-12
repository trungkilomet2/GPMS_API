namespace GMPS.API.DTOs
{
    public class LeaveRequestListDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? Content { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime? DateReply { get; set; }
        public string? DenyContent { get; set; }
        public string? Status { get; set; }
    }
}