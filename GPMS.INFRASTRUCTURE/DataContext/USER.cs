using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("USERNAME", Name = "UQ__USER__B15BE12E7DEEB8A4", IsUnique = true)]
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

    [InverseProperty("USER")]
    public virtual ICollection<CUTTING_NOTEBOOK_LOG> CUTTING_NOTEBOOK_LOG { get; set; } = new List<CUTTING_NOTEBOOK_LOG>();

    [InverseProperty("USER")]
    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUEST { get; set; } = new List<LEAVE_REQUEST>();

    [InverseProperty("USER")]
    public virtual ICollection<ORDER> ORDER { get; set; } = new List<ORDER>();

    [InverseProperty("USER")]
    public virtual ICollection<ORDER_REJECT_REASON> ORDER_REJECT_REASON { get; set; } = new List<ORDER_REJECT_REASON>();

    [InverseProperty("USER")]
    public virtual ICollection<PART_WORK_LOG> PART_WORK_LOG { get; set; } = new List<PART_WORK_LOG>();

    [InverseProperty("PM")]
    public virtual ICollection<PRODUCTION> PRODUCTION { get; set; } = new List<PRODUCTION>();

    [InverseProperty("USER")]
    public virtual ICollection<PRODUCTION_REJECT_REASON> PRODUCTION_REJECT_REASON { get; set; } = new List<PRODUCTION_REJECT_REASON>();

    [InverseProperty("TEAM_LEADER")]
    public virtual ICollection<P_PART> P_PART { get; set; } = new List<P_PART>();

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
    public virtual ICollection<WORKER_ROLE> WR { get; set; } = new List<WORKER_ROLE>();
}
