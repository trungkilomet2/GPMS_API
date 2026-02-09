using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART_OUTPUT
{
    public int PPO_ID { get; set; }

    public int PP_ID { get; set; }

    public DateTime? DATETIME_SUBMIT { get; set; }

    public string? UOM { get; set; }

    public decimal? VALUE { get; set; }

    public decimal? TRUE_VALUE { get; set; }

    public decimal? FALSE_VALUE { get; set; }

    public DateTime? DATETIME_VALIDATE { get; set; }

    public int PPOS_ID { get; set; }

    public virtual P_PART PP { get; set; } = null!;

    public virtual P_PART_OUTPUT_STATUS PPOS { get; set; } = null!;
}
