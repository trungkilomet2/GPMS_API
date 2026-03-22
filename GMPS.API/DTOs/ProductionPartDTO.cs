namespace GMPS.API.DTOs
{
    public class ProductionPartDTO
    {
        public string PartName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cpu { get; set; }
        public int StatusId { get; set; }
    }
}
