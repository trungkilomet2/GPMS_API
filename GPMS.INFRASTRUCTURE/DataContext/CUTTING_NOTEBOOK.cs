using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class CUTTING_NOTEBOOK
{
    [Key]
    public int CP_ID { get; set; }

    public int PRODUCTION_ID { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MARKER_LENGTH { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FABRIC_WIDTH { get; set; }

    [InverseProperty("CP")]
    public virtual ICollection<CUTTING_NOTEBOOK_LOG> CUTTING_NOTEBOOK_LOG { get; set; } = new List<CUTTING_NOTEBOOK_LOG>();

    [ForeignKey("PRODUCTION_ID")]
    [InverseProperty("CUTTING_NOTEBOOK")]
    public virtual PRODUCTION PRODUCTION { get; set; } = null!;
}
