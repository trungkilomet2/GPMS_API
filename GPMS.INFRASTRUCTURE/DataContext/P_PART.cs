using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART
{
    public int PP_ID { get; set; }

    public string NAME { get; set; } = null!;

    public DateTime START_TIME { get; set; }

    public DateTime END_TIME { get; set; }

    public decimal CPU { get; set; }

    public virtual P_PLAN PP { get; set; } = null!;

    public virtual ICollection<PP_ASSIGNEE> PP_ASSIGNEEs { get; set; } = new List<PP_ASSIGNEE>();

    public virtual ICollection<P_PART_OUTPUT> P_PART_OUTPUTs { get; set; } = new List<P_PART_OUTPUT>();
}
