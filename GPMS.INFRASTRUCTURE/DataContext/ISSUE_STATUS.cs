using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ISSUE_STATUS
{
    [Key]
    public int IS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("IS")]
    public virtual ICollection<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOG { get; set; } = new List<PRODUCTION_ISSUE_LOG>();
}
