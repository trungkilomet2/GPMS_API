using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class U_PAYROLL_OUTPUT_STATUS
{
    public int UPOS_ID { get; set; }

    public string NAME { get; set; } = null!;

    public virtual ICollection<U_PAYROLL_OUTPUT> U_PAYROLL_OUTPUTs { get; set; } = new List<U_PAYROLL_OUTPUT>();
}
