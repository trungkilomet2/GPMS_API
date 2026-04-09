using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class PART_WORK_LOG
{
    [Key]
    public int WL_ID { get; set; }

    public int PPOS_ID { get; set; }

    public int USER_ID { get; set; }

    public int QUANTITY { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CREATE_DATE { get; set; }

    public bool? IS_READ_ONLY { get; set; }

    public bool? IS_PAYMENT { get; set; }

    [StringLength(255)]
    public string? NOTE { get; set; }

    [ForeignKey("PPOS_ID")]
    [InverseProperty("PART_WORK_LOG")]
    public virtual P_PART_ORDER_SIZE PPOS { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("PART_WORK_LOG")]
    public virtual USER USER { get; set; } = null!;
}
