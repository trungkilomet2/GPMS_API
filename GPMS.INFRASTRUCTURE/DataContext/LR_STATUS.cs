using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class LR_STATUS
{
    [Key]
    public int LRS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("LRS")]
    public virtual ICollection<LEAVE_REQUEST> LEAVE_REQUEST { get; set; } = new List<LEAVE_REQUEST>();
}
