namespace GPMS.DOMAIN.Entities
{
    public class OHistoryUpdate
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}