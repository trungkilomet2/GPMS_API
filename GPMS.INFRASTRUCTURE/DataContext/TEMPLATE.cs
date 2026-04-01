using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class TEMPLATE
{
    [Key]
    public int TEMPLATE_ID { get; set; }

    [StringLength(100)]
    public string TEMPLATE_NAME { get; set; } = null!;

    [InverseProperty("TEMPLATE")]
    public virtual ICollection<TEMPLATE_STEP> TEMPLATE_STEP { get; set; } = new List<TEMPLATE_STEP>();
}
