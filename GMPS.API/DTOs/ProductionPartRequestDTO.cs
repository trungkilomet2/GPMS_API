using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateProductionPartItemDTO
    {
        [Required]
        [StringLength(150)]
        public string PartName { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(100, double.MaxValue)]
        public decimal Cpu { get; set; }

        
    }

    public class CreateProductionPartListDTO
    {
        [Required]
        [MinLength(1)]
        public List<CreateProductionPartItemDTO> Parts { get; set; } = new();
    }

    public class UpdateProductionPartDTO : CreateProductionPartItemDTO
    {
        [Range(1, 3)]
        public int StatusId { get; set; }
    }

    public class AssignProductionPartWorkersDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> WorkerIds { get; set; } = new();
    }
}
