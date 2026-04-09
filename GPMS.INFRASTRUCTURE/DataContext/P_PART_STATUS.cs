using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("NAME", Name = "UQ__P_PART_S__D9C1FA00C5BFA3CC", IsUnique = true)]
public partial class P_PART_STATUS
{
    [Key]
    public int PPS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("PPS")]
    public virtual ICollection<P_PART> P_PART { get; set; } = new List<P_PART>();
}
