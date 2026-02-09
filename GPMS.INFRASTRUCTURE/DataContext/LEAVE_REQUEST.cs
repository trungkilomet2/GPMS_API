using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class LEAVE_REQUEST
{
    public int LR_ID { get; set; }

    public int USER_ID { get; set; }

    public string? CONTENT { get; set; }

    public DateTime? DATE_CREATE { get; set; }

    public DateTime? DATE_REPLY { get; set; }

    public string? DENY_CONTENT { get; set; }

    public int LRS_ID { get; set; }

    public virtual LR_STATUS LRS { get; set; } = null!;

    public virtual USER USER { get; set; } = null!;
}
