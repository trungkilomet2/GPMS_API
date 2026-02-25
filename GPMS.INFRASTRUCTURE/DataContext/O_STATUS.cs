using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class O_STATUS
{
    [Key]
    public int OS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("OS")]
    public virtual ICollection<ORDER> ORDER { get; set; } = new List<ORDER>();
}
