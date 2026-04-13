using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Index("PRODUCTION_ID", Name = "UQ__PRODUCTI__4E709CABF287972B", IsUnique = true)]
public partial class PRODUCTION_REJECT_REASON
{
    [Key]
    public int ORR_ID { get; set; }

    public int PRODUCTION_ID { get; set; }

    public int USER_ID { get; set; }

    [StringLength(150)]
    public string? REASON { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CREATED_AT { get; set; }

    [ForeignKey("PRODUCTION_ID")]
    [InverseProperty("PRODUCTION_REJECT_REASON")]
    public virtual PRODUCTION PRODUCTION { get; set; } = null!;

    [ForeignKey("USER_ID")]
    [InverseProperty("PRODUCTION_REJECT_REASON")]
    public virtual USER USER { get; set; } = null!;
}
