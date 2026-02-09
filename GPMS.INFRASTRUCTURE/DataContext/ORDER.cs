using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ORDER
{
    public int ORDER_ID { get; set; }

    public int USER_ID { get; set; }

    public string? TYPE { get; set; }

    public string? SIZE { get; set; }

    public string? COLOR { get; set; }

    public DateOnly? START_DATE { get; set; }

    public DateOnly? END_DATE { get; set; }

    public int? QUANTITY { get; set; }

    public decimal? CPU { get; set; }

    public string? NOTE { get; set; }

    public int OS_ID { get; set; }

    public virtual O_STATUS OS { get; set; } = null!;

    public virtual ICollection<O_HISTORY_UPDATE> O_HISTORY_UPDATEs { get; set; } = new List<O_HISTORY_UPDATE>();

    public virtual O_MATERIAL? O_MATERIAL { get; set; }

    public virtual O_TEMPLATE? O_TEMPLATE { get; set; }

    public virtual ICollection<PRODUCTION> PRODUCTIONs { get; set; } = new List<PRODUCTION>();

    public virtual ICollection<UO_COMMENT> UO_COMMENTs { get; set; } = new List<UO_COMMENT>();

    public virtual USER USER { get; set; } = null!;
}
