using GPMS.DOMAIN.Entities;

namespace GMPS.API.DTOs
{
    public class ProductionDetailDTO
    {
        public int ProductionId { get; set; } 
        public ProductionDetailPMDTO Pm { get; set; }
        public ProductionDetailOrderDTO Order { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int StatusId { get; set; }
    }
    
    public class ProductionDetailPMDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        public string AvartarUrl { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }    
    }

    public class ProductionDetailOrderDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Image { get; set; }
        public string OrderName { get; set; }
        public IEnumerable<ProductionDetailOrderSizeDTO> OrderSizes { get; set; } = new List<ProductionDetailOrderSizeDTO>();
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public List<OrderTemplate>? Templates { get; set; }
        public List<OrderMaterial>? Materials { get; set; }

    }

    public class ProductionDetailOrderSizeDTO
    {
        public int SizeId { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int OrderSizeStatusId { get; set; }
        public string OrderSizeStatusName { get; set; }

    }




}
