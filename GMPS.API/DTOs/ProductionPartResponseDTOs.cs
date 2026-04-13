using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ProductionPartDetailDTO
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cpu { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public IEnumerable<ProductionPartOrderSize> ListPartOrderSizes { get; set; } = new List<ProductionPartOrderSize>();
    }


    public class ProductionPartUserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class ProductionPartWorkLogResponseDTO
    {
        public int WorkLogId { get; set; }
        public int PartId { get; set; }
        public int PartOrderSizeId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public int UserId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsPayment { get; set; }
    }

    public class DeliveryResponseDTO
    {
        public int Id { get; set; }
        public int OrderSizeId { get; set; }
        public int DeliverQuantity { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int DeliverStatusId { get; set; }
        public string? Color { get; set; }
        public string? SizeName { get; set; }

    }

}
