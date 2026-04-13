using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class D_STATUS
{
    [Key]
    public int DS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("DS")]
    public virtual ICollection<DELIVERY> DELIVERY { get; set; } = new List<DELIVERY>();
}
