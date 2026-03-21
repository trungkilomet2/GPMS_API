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
        public string Name { get; set; } = string.Empty;
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
        public string Type { get; set; }
        public string? Size { get; set; }
        public string Color { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Quantity { get; set; }
        public decimal? Cpu { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public IEnumerable<OTemplate> Templates { get; set; } = new List<OTemplate>();
        public IEnumerable<OMaterial> Materials { get; set; } = new List<OMaterial>();

    }




}
