namespace GPMS.DOMAIN.Entities
{
    public class Production
    {
        public int Id { get; set; }
        public int PmId { get; set; }
        public int OrderId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? RejectReason { get; set; }
        public List<ProductionPart> Parts { get; set; } = new();
    }
}