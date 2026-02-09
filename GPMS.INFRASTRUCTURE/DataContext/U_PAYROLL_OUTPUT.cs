using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class U_PAYROLL_OUTPUT
{
    public int UP_ID { get; set; }

    public int? USER_ID { get; set; }

    public decimal? TOTAL_PRICE { get; set; }

    public DateOnly? PAYMENT_DATE { get; set; }

    public string? NOTE { get; set; }

    public int UPOS_ID { get; set; }

    public virtual U_PAYROLL_OUTPUT_STATUS UPOS { get; set; } = null!;

    public virtual USER? USER { get; set; }
}
