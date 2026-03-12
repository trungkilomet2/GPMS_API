using GPMS.DOMAIN.Entities;
using System;
using System.ComponentModel;

namespace GMPS.API.DTOs
{
    public class LeaveRequestRequestDTO : RequestDTO<LeaveRequest>
    {
        [DefaultValue(null)]
        public string? Status { get; set; } = null;

        public DateTime? DateCreateFrom { get; set; } = null;

        public DateTime? DateCreateTo { get; set; } = null;
    }
}