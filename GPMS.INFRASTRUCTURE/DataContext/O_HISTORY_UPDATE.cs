using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_HISTORY_UPDATE
{
    [Key]
    public int OHU_ID { get; set; }

    public int ORDER_ID { get; set; }

    [StringLength(100)]
    public string FIELD_NAME { get; set; } = null!;

    [StringLength(255)]
    public string OLD_VALUE { get; set; } = null!;

    [StringLength(255)]
    public string NEW_VALUE { get; set; } = null!;

    [ForeignKey("ORDER_ID")]
    [InverseProperty("O_HISTORY_UPDATE")]
    public virtual ORDER ORDER { get; set; } = null!;
}
