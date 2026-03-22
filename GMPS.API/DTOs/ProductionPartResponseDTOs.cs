namespace GMPS.API.DTOs
{
    public class ProductionPartUserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

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
        public IEnumerable<ProductionPartUserDTO> Assignees { get; set; } = new List<ProductionPartUserDTO>();
    }
}
