using GPMS.DOMAIN.Entities;
using System;
using System.ComponentModel;

namespace GMPS.API.DTOs
{
    public class OrderRequestDTO : RequestDTO<Order>
    {
        [DefaultValue(null)]
        public string? Status { get; set; } = null;

        public DateOnly? StartDateFrom { get; set; } = null;

        public DateOnly? StartDateTo { get; set; } = null;

        [DefaultValue(null)]
        public string? SortColumn2 { get; set; } = null;

        [DefaultValue("ASC")]
        public string? SortOrder2 { get; set; } = "ASC";
    }
}
