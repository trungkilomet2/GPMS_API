using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateCuttingNotebookDTO
    {
        [Range(1, int.MaxValue)]
        public int ProductionId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal MarkerLength { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal FabricWidth { get; set; }
    }

    public class CreateCuttingNotebookLogDTO
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [StringLength(30)]
        public string Color { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal MeterPerKg { get; set; }

        [Range(1, int.MaxValue)]
        public int Layer { get; set; }

        [Range(1, int.MaxValue)]
        public int ProductQty { get; set; }

        [Range(0.0, double.MaxValue)]
        public decimal? AvgConsumption { get; set; }

        [StringLength(150)]
        public string? Note { get; set; }
    }
}