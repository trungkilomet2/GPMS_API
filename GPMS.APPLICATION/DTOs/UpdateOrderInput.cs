using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;

namespace GPMS.APPLICATION.DTOs
{
    public class UpdateOrderInput
    {
        public string OrderName { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Size { get; set; }
        public string Color { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public string? Note { get; set; }
        public List<OrderTemplate>? Templates { get; set; }
        public List<OrderMaterial>? Materials { get; set; }
    }
}
