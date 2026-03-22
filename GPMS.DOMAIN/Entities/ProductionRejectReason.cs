using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class ProductionRejectReason
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

}
