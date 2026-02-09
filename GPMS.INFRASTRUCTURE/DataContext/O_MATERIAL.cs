using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_MATERIAL
{
    public int ORDER_ID { get; set; }

    public string? NAME { get; set; }

    public string? IMAGE { get; set; }

    public decimal? VALUE { get; set; }

    public string? UOM { get; set; }

    public string? NOTE { get; set; }

    public virtual ORDER ORDER { get; set; } = null!;
}
