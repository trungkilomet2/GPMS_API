using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateProductionPartItemDTO
    {
        [Required]
        [StringLength(150)]
        public string PartName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TeamLeaderId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Cpu { get; set; }

        [Range(1, int.MaxValue)]
        public int StatusId { get; set; }
    }

    public class CreateProductionPartListDTO
    {
        [Required]
        [MinLength(1)]
        public List<CreateProductionPartItemDTO> Parts { get; set; } = new();
    }

    public class UpdateProductionPartDTO : CreateProductionPartItemDTO
    {
    }

    public class AssignProductionPartWorkersDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> WorkerIds { get; set; } = new();
    }
}
