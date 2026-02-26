using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ORDER
{
    [Key]
    public int ORDER_ID { get; set; }

    public int USER_ID { get; set; }

    [StringLength(255)]
    public string? IMAGE { get; set; }

    [StringLength(100)]
    public string ORDER_NAME { get; set; } = null!;

    [StringLength(50)]
    public string TYPE { get; set; } = null!;

    [StringLength(5)]
    [Unicode(false)]
    public string? SIZE { get; set; }

    [StringLength(30)]
    public string COLOR { get; set; } = null!;

    public DateOnly START_DATE { get; set; }

    public DateOnly END_DATE { get; set; }

    public int QUANTITY { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CPU { get; set; }

    [StringLength(255)]
    public string? NOTE { get; set; }

    public int OS_ID { get; set; }

    [ForeignKey("OS_ID")]
    [InverseProperty("ORDER")]
    public virtual O_STATUS OS { get; set; } = null!;

    [InverseProperty("ORDER")]
    public virtual ICollection<O_HISTORY_UPDATE> O_HISTORY_UPDATE { get; set; } = new List<O_HISTORY_UPDATE>();

    [InverseProperty("ORDER")]
    public virtual ICollection<O_MATERIAL> O_MATERIAL { get; set; } = new List<O_MATERIAL>();

    [InverseProperty("ORDER")]
    public virtual ICollection<O_TEMPLATE> O_TEMPLATE { get; set; } = new List<O_TEMPLATE>();

    [InverseProperty("TO_ORDERNavigation")]
    public virtual ICollection<UO_COMMENT> UO_COMMENT { get; set; } = new List<UO_COMMENT>();

    [ForeignKey("USER_ID")]
    [InverseProperty("ORDER")]
    public virtual USER USER { get; set; } = null!;
}
