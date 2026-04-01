using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART
{
    [Key]
    public int PP_ID { get; set; }

    public int PRODUCTION_ID { get; set; }

    [StringLength(150)]
    public string PART_NAME { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime START_DATE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime END_DATE { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CPU { get; set; }

    public int PPS_ID { get; set; }

    [InverseProperty("PP")]
    public virtual ICollection<PART_WORK_LOG> PART_WORK_LOG { get; set; } = new List<PART_WORK_LOG>();

    [ForeignKey("PPS_ID")]
    [InverseProperty("P_PART")]
    public virtual P_PART_STATUS PPS { get; set; } = null!;

    [ForeignKey("PRODUCTION_ID")]
    [InverseProperty("P_PART")]
    public virtual PRODUCTION PRODUCTION { get; set; } = null!;

    [InverseProperty("PART")]
    public virtual ICollection<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOG { get; set; } = new List<PRODUCTION_ISSUE_LOG>();

    [ForeignKey("PP_ID")]
    [InverseProperty("PP")]
    public virtual ICollection<USER> USER { get; set; } = new List<USER>();
}
