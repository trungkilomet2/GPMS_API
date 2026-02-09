using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class LR_STATUS
{
    public int LRS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUESTs { get; set; } = new List<LEAVE_REQUEST>();
}
