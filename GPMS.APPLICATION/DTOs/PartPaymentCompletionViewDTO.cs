using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class PartPaymentCompletionViewDTO
    {
        public int PartId { get; set; }
        public int AffectedLogs { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
