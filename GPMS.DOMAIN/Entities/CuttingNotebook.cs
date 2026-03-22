using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class CuttingNotebook
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public decimal MarkerLength { get; set; }
        public decimal FabricWidth { get; set; }
    }
}
