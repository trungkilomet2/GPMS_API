using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ROLE
{
    [Key]
    public int ROLE_ID { get; set; }

    [StringLength(30)]
    public string NAME { get; set; } = null!;

    [ForeignKey("ROLE_ID")]
    [InverseProperty("ROLE")]
    public virtual ICollection<USER> USER { get; set; } = new List<USER>();
}
