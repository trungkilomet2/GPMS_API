using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_TEMPLATE
{
    [Key]
    public int OT_ID { get; set; }

    public int ORDER_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [StringLength(50)]
    public string? TYPE { get; set; }

    [StringLength(255)]
    public string? FILE { get; set; }

    public int? QUANTITY { get; set; }

    [StringLength(100)]
    public string? NOTE { get; set; }

    [ForeignKey("ORDER_ID")]
    [InverseProperty("O_TEMPLATE")]
    public virtual ORDER ORDER { get; set; } = null!;
}
