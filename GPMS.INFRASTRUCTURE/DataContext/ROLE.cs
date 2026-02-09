using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ROLE
{
    public int ROLE_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<USER> USERs { get; set; } = new List<USER>();
}
