namespace GPMS.DOMAIN.Entities
{
    public class OMaterial
    {
        public int Id { get; set; }
        public int OrderId { get; set; }   
        public string Name { get; set; }
        public string? Image { get; set; }
        public decimal Value { get; set; }
        public string Uom { get; set; }
        public string? Note { get; set; }
    }
}