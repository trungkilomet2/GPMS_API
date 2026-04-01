using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class WORKER_SKILL
{
    [Key]
    public int WS_ID { get; set; }

    [StringLength(50)]
    public string NAME { get; set; } = null!;

    [ForeignKey("WS_ID")]
    [InverseProperty("WS")]
    public virtual ICollection<USER> USER { get; set; } = new List<USER>();
}
