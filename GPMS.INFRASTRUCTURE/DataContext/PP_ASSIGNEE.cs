using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PP_ASSIGNEE
{
    public int PPA_ID { get; set; }

    public int PP_ID { get; set; }

    public int USER_ID { get; set; }

    public DateTime? DATE_ASSIGN { get; set; }

    public int PPAS_ID { get; set; }

    public virtual P_PART PP { get; set; } = null!;

    public virtual PP_ASSIGNEE_STATUS PPAS { get; set; } = null!;

    public virtual USER USER { get; set; } = null!;
}
