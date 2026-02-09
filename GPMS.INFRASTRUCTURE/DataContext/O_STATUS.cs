using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_STATUS
{
    public int OS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<ORDER> ORDERs { get; set; } = new List<ORDER>();
}
