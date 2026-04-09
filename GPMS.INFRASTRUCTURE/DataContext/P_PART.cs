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

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CPU { get; set; }

    public int PPS_ID { get; set; }

    [ForeignKey("PPS_ID")]
    [InverseProperty("P_PART")]
    public virtual P_PART_STATUS PPS { get; set; } = null!;

    [ForeignKey("PRODUCTION_ID")]
    [InverseProperty("P_PART")]
    public virtual PRODUCTION PRODUCTION { get; set; } = null!;

    [InverseProperty("PP")]
    public virtual ICollection<P_PART_ORDER_SIZE> P_PART_ORDER_SIZE { get; set; } = new List<P_PART_ORDER_SIZE>();
}
