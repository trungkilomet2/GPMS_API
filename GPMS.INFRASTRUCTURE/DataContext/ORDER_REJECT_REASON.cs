using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("ORDER_ID", Name = "UQ__ORDER_RE__460A94654BDBF808", IsUnique = true)]
public partial class ORDER_REJECT_REASON
{
    [Key]
    public int ORR_ID { get; set; }

    public int ORDER_ID { get; set; }

    public int USER_ID { get; set; }

    [StringLength(150)]
    public string? REASON { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CREATED_AT { get; set; }

    [ForeignKey("ORDER_ID")]
    [InverseProperty("ORDER_REJECT_REASON")]
    public virtual ORDER ORDER { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("ORDER_REJECT_REASON")]
    public virtual USER USER { get; set; } = null!;
}
