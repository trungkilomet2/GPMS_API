using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class CuttingNotebookLog
    {
        public int Id { get; set; }
        public int NotebookId { get; set; }
        public int UserId { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal MeterPerKg { get; set; }
        public int Layer { get; set; }
        public int ProductQty { get; set; }
        public decimal? AvgConsumption { get; set; }
        public string? Note { get; set; }
        public DateOnly? DateCreate { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsPayment { get; set; }

    }
}
