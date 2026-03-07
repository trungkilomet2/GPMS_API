using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Image { get; set; }
        public string OrderName { get; set; }
        public string Type { get; set; }
        public string? Size { get; set; }
        public string Color { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }
        public IEnumerable<OTemplate> Templates { get; set; } = new List<OTemplate>();
        public IEnumerable<OMaterial> Materials { get; set; } = new List<OMaterial>();
        public IEnumerable<OHistoryUpdate> Histories { get; set; } = new List<OHistoryUpdate>();
        public List<OrderMaterial>? Material { get; set; }

        public List<OrderTemplate>? Template { get; set; }
    }
}
