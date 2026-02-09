using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_HISTORY_UPDATE
{
    public int OHU_ID { get; set; }

    public int? ORDER_ID { get; set; }

    public string? FIELD_NAME { get; set; }

    public string? OLD_VALUE { get; set; }

    public string? NEW_VALUE { get; set; }

    public virtual ORDER? ORDER { get; set; }
}
