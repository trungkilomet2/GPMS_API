using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class DeliveryPlanningItemViewDTO
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
