using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART_OUTPUT_STATUS
{
    public int PPOS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<P_PART_OUTPUT> P_PART_OUTPUTs { get; set; } = new List<P_PART_OUTPUT>();
}
