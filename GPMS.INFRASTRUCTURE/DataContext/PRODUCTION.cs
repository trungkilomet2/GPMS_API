using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PRODUCTION
{
    [Key]
    public int PRODUCTION_ID { get; set; }

    public int PM_ID { get; set; }

    public int ORDER_ID { get; set; }

    public DateOnly? P_START_DATE { get; set; }

    public DateOnly? P_END_DATE { get; set; }

    public int PS_ID { get; set; }

    [InverseProperty("PRODUCTION")]
    public virtual ICollection<CUTTING_NOTEBOOK> CUTTING_NOTEBOOK { get; set; } = new List<CUTTING_NOTEBOOK>();

    [ForeignKey("ORDER_ID")]
    [InverseProperty("PRODUCTION")]
    public virtual ORDER ORDER { get; set; } = null!;

    [ForeignKey("PM_ID")]
    [InverseProperty("PRODUCTION")]
    public virtual USER PM { get; set; } = null!;

    [InverseProperty("PRODUCTION")]
    public virtual PRODUCTION_REJECT_REASON? PRODUCTION_REJECT_REASON { get; set; }

    [ForeignKey("PS_ID")]
    [InverseProperty("PRODUCTION")]
    public virtual P_STATUS PS { get; set; } = null!;

    [InverseProperty("PRODUCTION")]
    public virtual ICollection<P_PART> P_PART { get; set; } = new List<P_PART>();

    [InverseProperty("TO_PRODUCTIONNavigation")]
    public virtual ICollection<UP_COMMENT> UP_COMMENT { get; set; } = new List<UP_COMMENT>();
}
