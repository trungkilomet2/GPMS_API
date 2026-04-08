using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class GUEST_ORDER
{
    [Key]
    public int GUEST_ID { get; set; }

    [StringLength(100)]
    public string? GUEST_NAME { get; set; }

    [StringLength(20)]
    public string? GUEST_PHONE { get; set; }

    [StringLength(255)]
    public string? GUEST_ADDRESS { get; set; }

    [InverseProperty("GUEST")]
    public virtual ICollection<ORDER> ORDER { get; set; } = new List<ORDER>();
}
