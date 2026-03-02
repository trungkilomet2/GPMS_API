using System;
using System.Collections.Generic;

namespace GPMS.DOMAIN.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Image { get; set; }
        public string OrderName { get; set; }
        public string Type { get; set; }
        public string? Size { get; set; }
        public string Color { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; }

        // Detail fields
        public List<OrderMaterial> Materials { get; set; } = new();
        public List<OrderSample> Samples { get; set; } = new();
    }

    public class OrderMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class OrderSample
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; }
        public string? File { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
    }
}