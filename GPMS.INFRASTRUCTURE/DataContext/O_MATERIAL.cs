using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_MATERIAL
{
    [Key]
    public int OM_ID { get; set; }

    public int ORDER_ID { get; set; }

    [StringLength(150)]
    public string NAME { get; set; } = null!;

    [StringLength(255)]
    public string? IMAGE { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal VALUE { get; set; }

    [StringLength(50)]
    public string UOM { get; set; } = null!;

    [StringLength(100)]
    public string? NOTE { get; set; }

    [ForeignKey("ORDER_ID")]
    [InverseProperty("O_MATERIAL")]
    public virtual ORDER ORDER { get; set; } = null!;
}
