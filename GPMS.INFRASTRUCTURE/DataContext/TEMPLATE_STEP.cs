using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class TEMPLATE_STEP
{
    [Key]
    public int STEP_ID { get; set; }

    public int TEMPLATE_ID { get; set; }

    [StringLength(100)]
    public string PRODUCTION_PART_NAME { get; set; } = null!;

    public int STEP_ORDER { get; set; }

    [ForeignKey("TEMPLATE_ID")]
    [InverseProperty("TEMPLATE_STEP")]
    public virtual TEMPLATE TEMPLATE { get; set; } = null!;
}
