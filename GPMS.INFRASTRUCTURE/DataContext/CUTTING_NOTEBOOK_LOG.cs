using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class CUTTING_NOTEBOOK_LOG
{
    [Key]
    public int CND_ID { get; set; }

    public int CP_ID { get; set; }

    public int USER_ID { get; set; }

    [StringLength(30)]
    public string COLOR { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal METER_PER_KG { get; set; }

    public int LAYER { get; set; }

    public int PRODUCT_QTY { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal? AVG_CONSUMPTION { get; set; }

    [StringLength(150)]
    public string? NOTE { get; set; }

    public DateOnly? DATE_CREATE { get; set; }

    public bool? IS_READ_ONLY { get; set; }

    public bool? IS_PAYMENT { get; set; }

    [ForeignKey("CP_ID")]
    [InverseProperty("CUTTING_NOTEBOOK_LOG")]
    public virtual CUTTING_NOTEBOOK CP { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("CUTTING_NOTEBOOK_LOG")]
    public virtual USER USER { get; set; } = null!;
}
