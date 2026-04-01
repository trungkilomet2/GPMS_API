using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("USERNAME", Name = "UQ__USER__B15BE12E42F4D952", IsUnique = true)]
public partial class USER
{
    [Key]
    public int USER_ID { get; set; }

    [StringLength(50)]
    public string? USERNAME { get; set; }

    [StringLength(255)]
    public string PASSWORDHASH { get; set; } = null!;

    [StringLength(50)]
    public string FULLNAME { get; set; } = null!;

    [StringLength(11)]
    public string? PHONE_NUMBER { get; set; }

    [StringLength(255)]
    public string? AVATAR { get; set; }

    [StringLength(255)]
    public string? LOCATION { get; set; }

    [StringLength(150)]
    public string? EMAIL { get; set; }

    public int US_ID { get; set; }

    public int? MANAGER_ID { get; set; }

    [InverseProperty("USER")]
    public virtual ICollection<CUTTING_NOTEBOOK_LOG> CUTTING_NOTEBOOK_LOG { get; set; } = new List<CUTTING_NOTEBOOK_LOG>();

    [InverseProperty("MANAGER")]
    public virtual ICollection<USER> InverseMANAGER { get; set; } = new List<USER>();

    [InverseProperty("APPROVED_BYNavigation")]
    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUESTAPPROVED_BYNavigation { get; set; } = new List<LEAVE_REQUEST>();

    [InverseProperty("USER")]
    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUESTUSER { get; set; } = new List<LEAVE_REQUEST>();

    [ForeignKey("MANAGER_ID")]
    [InverseProperty("InverseMANAGER")]
    public virtual USER? MANAGER { get; set; }

    [InverseProperty("USER")]
    public virtual ICollection<ORDER> ORDER { get; set; } = new List<ORDER>();

    [InverseProperty("USER")]
    public virtual ICollection<ORDER_REJECT_REASON> ORDER_REJECT_REASON { get; set; } = new List<ORDER_REJECT_REASON>();

    [InverseProperty("USER")]
    public virtual ICollection<PART_WORK_LOG> PART_WORK_LOG { get; set; } = new List<PART_WORK_LOG>();

    [InverseProperty("PM")]
    public virtual ICollection<PRODUCTION> PRODUCTION { get; set; } = new List<PRODUCTION>();

    [InverseProperty("ASSIGNED_TONavigation")]
    public virtual ICollection<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOGASSIGNED_TONavigation { get; set; } = new List<PRODUCTION_ISSUE_LOG>();

    [InverseProperty("CREATED_BYNavigation")]
    public virtual ICollection<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOGCREATED_BYNavigation { get; set; } = new List<PRODUCTION_ISSUE_LOG>();

    [InverseProperty("USER")]
    public virtual ICollection<PRODUCTION_REJECT_REASON> PRODUCTION_REJECT_REASON { get; set; } = new List<PRODUCTION_REJECT_REASON>();

    [InverseProperty("FROM_USERNavigation")]
    public virtual ICollection<UO_COMMENT> UO_COMMENT { get; set; } = new List<UO_COMMENT>();

    [InverseProperty("FROM_USERNavigation")]
    public virtual ICollection<UP_COMMENT> UP_COMMENT { get; set; } = new List<UP_COMMENT>();

    [ForeignKey("US_ID")]
    [InverseProperty("USER")]
    public virtual U_STATUS US { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("USER")]
    public virtual ICollection<P_PART> PP { get; set; } = new List<P_PART>();

    [ForeignKey("USER_ID")]
    [InverseProperty("USER")]
    public virtual ICollection<ROLE> ROLE { get; set; } = new List<ROLE>();

    [ForeignKey("USER_ID")]
    [InverseProperty("USER")]
    public virtual ICollection<WORKER_SKILL> WS { get; set; } = new List<WORKER_SKILL>();
}
