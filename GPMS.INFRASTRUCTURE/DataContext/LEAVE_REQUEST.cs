using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class LEAVE_REQUEST
{
    [Key]
    public int LR_ID { get; set; }

    public int USER_ID { get; set; }

    [StringLength(155)]
    public string? CONTENT { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DATE_CREATE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FROM_DATE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TO_DATE { get; set; }

    public int APPROVED_BY { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DATE_REPLY { get; set; }

    [StringLength(100)]
    public string? DENY_CONTENT { get; set; }

    public int LRS_ID { get; set; }

    [ForeignKey("APPROVED_BY")]
    [InverseProperty("LEAVE_REQUESTAPPROVED_BYNavigation")]
    public virtual USER APPROVED_BYNavigation { get; set; } = null!;

    [ForeignKey("LRS_ID")]
    [InverseProperty("LEAVE_REQUEST")]
    public virtual LR_STATUS LRS { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("LEAVE_REQUESTUSER")]
    public virtual USER USER { get; set; } = null!;
}
