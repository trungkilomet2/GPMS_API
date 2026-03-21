using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

[Keyless]
public partial class LOG_EVENTS
{
    public int ID { get; set; }

    public string? MESSAGE { get; set; }

    public string? MESSAGE_TEMPLATE { get; set; }

    public string? LEVEL { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? TIMPESTAMP { get; set; }

    public string? EXCEPTION { get; set; }

    public string? PROPERTIES { get; set; }
}
