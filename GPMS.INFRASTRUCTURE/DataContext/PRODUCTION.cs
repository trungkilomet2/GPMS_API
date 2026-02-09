using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PRODUCTION
{
    public int PRODUCTION_ID { get; set; }

    public int PM_ID { get; set; }

    public int ORDER_ID { get; set; }

    public int PS_ID { get; set; }

    public virtual ORDER ORDER { get; set; } = null!;

    public virtual USER PM { get; set; } = null!;

    public virtual P_STATUS PS { get; set; } = null!;

    public virtual ICollection<P_PLAN> P_PLANs { get; set; } = new List<P_PLAN>();

    public virtual ICollection<UP_COMMENT> UP_COMMENTs { get; set; } = new List<UP_COMMENT>();
}
