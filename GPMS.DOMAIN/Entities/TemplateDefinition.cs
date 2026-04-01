using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    namespace GPMS.DOMAIN.Entities
    {
        public class TemplateDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<TemplateStepDefinition> Steps { get; set; } = new();
        }

        public class TemplateStepDefinition
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string PartName { get; set; } = string.Empty;
        }
 
    }
}
