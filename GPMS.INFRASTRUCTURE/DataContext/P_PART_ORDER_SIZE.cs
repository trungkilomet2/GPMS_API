using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART_ORDER_SIZE
{
    [Key]
    public int PPOS_ID { get; set; }

    public int PP_ID { get; set; }

    [StringLength(50)]
    public string SIZE { get; set; } = null!;

    public int QUANTITY { get; set; }

    [StringLength(30)]
    public string COLOR { get; set; } = null!;

    public int PPOSS_ID { get; set; }

    [InverseProperty("PPOS")]
    public virtual ICollection<PART_WORK_LOG> PART_WORK_LOG { get; set; } = new List<PART_WORK_LOG>();

    [ForeignKey("PP_ID")]
    [InverseProperty("P_PART_ORDER_SIZE")]
    public virtual P_PART PP { get; set; } = null!;

    [ForeignKey("PPOSS_ID")]
    [InverseProperty("P_PART_ORDER_SIZE")]
    public virtual P_PART_ORDER_SIZE_STATUS PPOSS { get; set; } = null!;

    [InverseProperty("PPOS")]
    public virtual ICollection<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOG { get; set; } = new List<PRODUCTION_ISSUE_LOG>();

    [ForeignKey("PPOS_ID")]
    [InverseProperty("PPOS")]
    public virtual ICollection<USER> USER { get; set; } = new List<USER>();
}
