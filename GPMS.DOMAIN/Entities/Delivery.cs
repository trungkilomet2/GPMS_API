using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class Delivery
    {
        public int Id { get; set; }
        public int OrderSizeId { get; set; }
        public int DeliverQuantity { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int DeliverStatusId { get; set; }
    }
}
