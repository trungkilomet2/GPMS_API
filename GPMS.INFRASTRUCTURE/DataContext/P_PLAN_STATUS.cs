using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PLAN_STATUS
{
    public int PPS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<P_PLAN> P_PLANs { get; set; } = new List<P_PLAN>();
}
