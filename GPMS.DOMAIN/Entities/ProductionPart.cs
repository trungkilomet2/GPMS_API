namespace GPMS.DOMAIN.Entities
{
    public class ProductionPart
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public int TeamLeaderId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal Cpu { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}
