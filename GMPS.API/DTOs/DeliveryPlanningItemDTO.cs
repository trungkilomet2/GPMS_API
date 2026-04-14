namespace GMPS.API.DTOs
{
    public class DeliveryPlanningItemDTO
    {
        public int OrderSizeId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public int TotalOrderedQuantity { get; set; }
        public int DeliveredQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public int MaxDeliverableQuantity { get; set; }
    }
}
