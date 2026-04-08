using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class ProductionPartOrderSize
    {
        public int Id { get; set; }   
        public int ProductionPartId { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Color { get; set; } = string.Empty;
        public int PartOrderSizeStatusId { get; set; }  
    }
}
