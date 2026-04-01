using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Keyless]
public partial class USER_AUTHORIZE
{
    public int ID { get; set; }

    [StringLength(50)]
    public string CONTROLLER { get; set; } = null!;

    [StringLength(8)]
    public string METHOD { get; set; } = null!;

    [StringLength(100)]
    public string ACTION { get; set; } = null!;

    [StringLength(50)]
    public string? ROLE_AUTHORIZE { get; set; }
}
