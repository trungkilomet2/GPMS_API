using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class OrderTemplate
    {
        public string TemplateName { get; set; }
        public string? Type { get; set; }
        public string? File { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
    }
}
