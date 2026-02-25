using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class UO_COMMENT
{
    [Key]
    public int OC_ID { get; set; }

    public int? FROM_USER { get; set; }

    public int? TO_ORDER { get; set; }

    [StringLength(500)]
    public string? CONTENT { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SEND_DATETIME { get; set; }

    [ForeignKey("FROM_USER")]
    [InverseProperty("UO_COMMENT")]
    public virtual USER? FROM_USERNavigation { get; set; }

    [ForeignKey("TO_ORDER")]
    [InverseProperty("UO_COMMENT")]
    public virtual ORDER? TO_ORDERNavigation { get; set; }
}
