using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_TEMPLATE
{
    public int ORDER_ID { get; set; }

    public string? TYPE { get; set; }

    public string? FILE { get; set; }

    public virtual ORDER ORDER { get; set; } = null!;
}
