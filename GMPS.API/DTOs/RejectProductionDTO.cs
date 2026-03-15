namespace GMPS.API.DTOs
{
    public class RejectProductionDTO
    {
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
