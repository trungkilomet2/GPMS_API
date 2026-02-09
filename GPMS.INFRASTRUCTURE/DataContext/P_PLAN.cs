using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PLAN
{
    public int PP_ID { get; set; }

    public int PRODUCTION_ID { get; set; }

    public DateOnly START_DATE { get; set; }

    public DateOnly END_DATE { get; set; }

    public int P_PLAN_STATUS { get; set; }

    public virtual PRODUCTION PRODUCTION { get; set; } = null!;

    public virtual P_PART? P_PART { get; set; }

    public virtual P_PLAN_STATUS P_PLAN_STATUSNavigation { get; set; } = null!;
}
