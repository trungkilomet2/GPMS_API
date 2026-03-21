using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class OrderRejectReason
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
