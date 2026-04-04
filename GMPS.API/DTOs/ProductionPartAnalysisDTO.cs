using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class EstimatePartCompletionRequestDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> WorkerIds { get; set; } = new();
    }

    public class ProductionPartCompletionEstimateDTO
    {
        public int PartId { get; set; }
        public int ProductionId { get; set; }
        public int RemainingQuantity { get; set; }
        public int EstimatedDailyCapacity { get; set; }
        public int EstimatedDaysToComplete { get; set; }
        public DateOnly EstimatedFinishDate { get; set; }
    }

    public class ProductionWorkerProgressChartDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public int TotalOutput { get; set; }
        public decimal ProgressPercent { get; set; }
    }

    public class WorkerProductivityScoreDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public int TotalOutput { get; set; }
        public int IssueCount { get; set; }
        public decimal ProductivityScore { get; set; }
    }
}
