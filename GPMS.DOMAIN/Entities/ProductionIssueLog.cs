using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class ProductionIssueLog
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; } = 2;
        public int StatusId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
