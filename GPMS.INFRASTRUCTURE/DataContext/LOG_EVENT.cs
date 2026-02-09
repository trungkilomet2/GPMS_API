using System;
using System.Collections.Generic;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class LOG_EVENT
{
    public int LOG_ID { get; set; }

    public string? MESSAGE { get; set; }

    public string? MESSAGE_TEMPLATE { get; set; }

    public string? LEVEL { get; set; }

    public DateTime? TIMESTAMP { get; set; }

    public string? EXCEPTION { get; set; }

    public string? PROPERTIES { get; set; }
}
