using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class P_PART_ORDER_SIZE_STATUS
{
    [Key]
    public int PPOSS_ID { get; set; }

    [StringLength(50)]
    public string NAME { get; set; } = null!;

    [InverseProperty("PPOSS")]
    public virtual ICollection<P_PART_ORDER_SIZE> P_PART_ORDER_SIZE { get; set; } = new List<P_PART_ORDER_SIZE>();
}
