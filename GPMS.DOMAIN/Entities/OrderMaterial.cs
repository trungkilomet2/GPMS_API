using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class OrderMaterial
    {
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; }
    }
}
