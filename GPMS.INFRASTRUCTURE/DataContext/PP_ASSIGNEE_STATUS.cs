using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PP_ASSIGNEE_STATUS
{
    public int PPAS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<PP_ASSIGNEE> PP_ASSIGNEEs { get; set; } = new List<PP_ASSIGNEE>();
}
