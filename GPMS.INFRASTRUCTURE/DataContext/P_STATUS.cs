using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_STATUS
{
    public int PS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<PRODUCTION> PRODUCTIONs { get; set; } = new List<PRODUCTION>();
}
