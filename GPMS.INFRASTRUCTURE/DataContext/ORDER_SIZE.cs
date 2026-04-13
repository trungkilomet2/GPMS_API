using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ORDER_SIZE
{
    [Key]
    public int OD_ID { get; set; }

    public int ORDER_ID { get; set; }

    public int SIZE_ID { get; set; }

    [StringLength(50)]
    public string COLOR { get; set; } = null!;

    public int QUANTITY { get; set; }

    public int OSS_ID { get; set; }

    [InverseProperty("ORDER_SIZE")]
    public virtual ICollection<DELIVERY> DELIVERY { get; set; } = new List<DELIVERY>();

    [ForeignKey("ORDER_ID")]
    [InverseProperty("ORDER_SIZE")]
    public virtual ORDER ORDER { get; set; } = null!;

    [ForeignKey("OSS_ID")]
    [InverseProperty("ORDER_SIZE")]
    public virtual ORDER_SIZE_STATUS OSS { get; set; } = null!;

    [ForeignKey("SIZE_ID")]
    [InverseProperty("ORDER_SIZE")]
    public virtual SIZE SIZE { get; set; } = null!;
}
