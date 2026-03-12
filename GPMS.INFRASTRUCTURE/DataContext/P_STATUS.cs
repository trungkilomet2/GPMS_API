using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("NAME", Name = "UQ__P_STATUS__D9C1FA00EE82091D", IsUnique = true)]
public partial class P_STATUS
{
    [Key]
    public int PS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("PS")]
    public virtual ICollection<PRODUCTION> PRODUCTION { get; set; } = new List<PRODUCTION>();
}
