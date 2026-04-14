using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class ProductionPartWorkLog
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public int PartOrderSizeId { get; set; }

        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public int UserId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsPayment { get; set; }
        public string? Note { get; set; }
        public string? PartName { get; set; }
        public string? WorkerName { get; set; }
    }
}
