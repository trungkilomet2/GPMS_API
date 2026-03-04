namespace GPMS.DOMAIN.Entities
{
    public class OTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? File { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
    }
}