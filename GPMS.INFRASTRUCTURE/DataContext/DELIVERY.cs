using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class DELIVERY
{
    [Key]
    public int DELIVERY_ID { get; set; }

    public int ORDER_SIZE_ID { get; set; }

    public int DELIVER_QUANTITY { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SHIPPED_DATE { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RECEIVED_DATE { get; set; }

    public int DS_ID { get; set; }

    [ForeignKey("DS_ID")]
    [InverseProperty("DELIVERY")]
    public virtual D_STATUS DS { get; set; } = null!;

    [ForeignKey("ORDER_SIZE_ID")]
    [InverseProperty("DELIVERY")]
    public virtual ORDER_SIZE ORDER_SIZE { get; set; } = null!;
}
