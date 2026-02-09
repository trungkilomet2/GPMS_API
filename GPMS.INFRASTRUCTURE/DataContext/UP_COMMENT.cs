using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class UP_COMMENT
{
    public int OC_ID { get; set; }

    public int? FROM_USER { get; set; }

    public int? TO_PO { get; set; }

    public string? CONTENT { get; set; }

    public DateTime? SEND_DATETIME { get; set; }

    public virtual USER? FROM_USERNavigation { get; set; }

    public virtual PRODUCTION? TO_PONavigation { get; set; }
}
