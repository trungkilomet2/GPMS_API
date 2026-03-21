namespace GMPS.API.DTOs
{
    public class ProductionPartDTO
    {
        public string PartName { get; set; } = string.Empty;
        public int TeamLeaderId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal Cpu { get; set; }
        public int StatusId { get; set; }
    }
}
