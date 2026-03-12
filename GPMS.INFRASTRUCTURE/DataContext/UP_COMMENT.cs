using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class UP_COMMENT
{
    [Key]
    public int UPC_ID { get; set; }

    public int FROM_USER { get; set; }

    public int TO_PRODUCTION { get; set; }

    [StringLength(500)]
    public string? CONTENT { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SEND_DATETIME { get; set; }

    [ForeignKey("FROM_USER")]
    [InverseProperty("UP_COMMENT")]
    public virtual USER FROM_USERNavigation { get; set; } = null!;

    [ForeignKey("TO_PRODUCTION")]
    [InverseProperty("UP_COMMENT")]
    public virtual PRODUCTION TO_PRODUCTIONNavigation { get; set; } = null!;
}
