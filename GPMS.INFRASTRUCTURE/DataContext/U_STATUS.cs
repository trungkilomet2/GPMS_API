using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class U_STATUS
{
    [Key]
    public int US_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("US")]
    public virtual ICollection<USER> USER { get; set; } = new List<USER>();
}
