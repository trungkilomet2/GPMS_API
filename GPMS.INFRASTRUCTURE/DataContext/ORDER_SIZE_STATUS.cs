using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class ORDER_SIZE_STATUS
{
    [Key]
    public int OSS_ID { get; set; }

    [StringLength(100)]
    public string NAME { get; set; } = null!;

    [InverseProperty("OSS")]
    public virtual ICollection<ORDER_SIZE> ORDER_SIZE { get; set; } = new List<ORDER_SIZE>();
}
