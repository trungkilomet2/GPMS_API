using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PRODUCTION_ISSUE_LOG
{
    [Key]
    public int ISSUE_ID { get; set; }

    [StringLength(150)]
    public string TITLE { get; set; } = null!;

    [StringLength(255)]
    public string? DESCRIPTION { get; set; }

    public int QUANTITY { get; set; }

    public int CREATED_BY { get; set; }

    public int ASSIGNED_TO { get; set; }

    public int PPOS_ID { get; set; }

    public int? PRIORITY { get; set; }

    [StringLength(255)]
    public string? IMAGE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CREATED_AT { get; set; }

    public int IS_ID { get; set; }

    [ForeignKey("ASSIGNED_TO")]
    [InverseProperty("PRODUCTION_ISSUE_LOGASSIGNED_TONavigation")]
    public virtual USER ASSIGNED_TONavigation { get; set; } = null!;

    [ForeignKey("CREATED_BY")]
    [InverseProperty("PRODUCTION_ISSUE_LOGCREATED_BYNavigation")]
    public virtual USER CREATED_BYNavigation { get; set; } = null!;

    [ForeignKey("IS_ID")]
    [InverseProperty("PRODUCTION_ISSUE_LOG")]
    public virtual ISSUE_STATUS IS { get; set; } = null!;

    [ForeignKey("PPOS_ID")]
    [InverseProperty("PRODUCTION_ISSUE_LOG")]
    public virtual P_PART_ORDER_SIZE PPOS { get; set; } = null!;
}
