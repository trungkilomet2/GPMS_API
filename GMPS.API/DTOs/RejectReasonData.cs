namespace GMPS.API.DTOs
{
    public class RejectReasonData
    {
        public int ProductionId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }


}
