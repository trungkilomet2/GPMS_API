using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class OrderSize
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int SizeId { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public int OrderSizeStatusId { get; set; }
    }
}
