using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class SIZE
{
    [Key]
    public int SIZE_ID { get; set; }

    [StringLength(20)]
    public string NAME { get; set; } = null!;

    [InverseProperty("SIZE")]
    public virtual ICollection<ORDER_SIZE> ORDER_SIZE { get; set; } = new List<ORDER_SIZE>();
}
