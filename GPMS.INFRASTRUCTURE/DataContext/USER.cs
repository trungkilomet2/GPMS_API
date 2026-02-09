using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class USER
{
    public int USER_ID { get; set; }

    public string FULLNAME { get; set; } = null!;

    public string PASSWORD { get; set; } = null!;

    public string? PHONE_NUMBER { get; set; }

    public string? LOCATION { get; set; }

    public string? EMAIL { get; set; }

    public int US_ID { get; set; }

    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUESTs { get; set; } = new List<LEAVE_REQUEST>();

    public virtual ICollection<ORDER> ORDERs { get; set; } = new List<ORDER>();

    public virtual ICollection<PP_ASSIGNEE> PP_ASSIGNEEs { get; set; } = new List<PP_ASSIGNEE>();

    public virtual ICollection<PRODUCTION> PRODUCTIONs { get; set; } = new List<PRODUCTION>();

    public virtual ICollection<UO_COMMENT> UO_COMMENTs { get; set; } = new List<UO_COMMENT>();

    public virtual ICollection<UP_COMMENT> UP_COMMENTs { get; set; } = new List<UP_COMMENT>();

    public virtual U_STATUS US { get; set; } = null!;

    public virtual ICollection<U_PAYROLL_OUTPUT> U_PAYROLL_OUTPUTs { get; set; } = new List<U_PAYROLL_OUTPUT>();

    public virtual ICollection<ROLE> ROLEs { get; set; } = new List<ROLE>();

    public virtual ICollection<WORKER_ROLE> WRs { get; set; } = new List<WORKER_ROLE>();
}
