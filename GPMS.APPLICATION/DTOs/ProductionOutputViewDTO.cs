using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    // DTO tầng Application: tổng hợp output của worker theo Production.
    public class ProductionWorkerOutputViewDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public int CuttingOutput { get; set; }
        public int SewingOutput { get; set; }
        public int TotalOutput => CuttingOutput + SewingOutput;
        public int IssueCount { get; set; }
    }
    // DTO tầng Application: một bản ghi lịch sử sản lượng theo worker.
    public class WorkerProductivityHistoryViewDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public string SourceType { get; set; } = string.Empty; // CuttingNotebookLog | PartWorkLog
        public int SourceId { get; set; } // Id bản ghi nguồn
        public int Quantity { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Note { get; set; }
    }

    // DTO tầng Application: output tổng Production.
    public class ProductionOutputSummaryViewDTO
    {
        public int ProductionId { get; set; }
        public int TotalCuttingOutput { get; set; }
        public int TotalSewingOutput { get; set; }
        public int TotalIssueCount { get; set; }
        public int TotalOutput => TotalCuttingOutput + TotalSewingOutput;
    }

    // DTO tầng Application: danh sách kế hoạch giao cho worker.
    public class WorkerAssignedPlanViewDTO
    {
        public int ProductionId { get; set; }
        public int OrderId { get; set; }
        public string OrderName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public IEnumerable<string> PartNames { get; set; } = new List<string>();
    }

    // 30-3-2026:


    public class ProductionPartCompletionEstimateViewDTO
    {
        public int PartId { get; set; }
        public int ProductionId { get; set; }
        public int RemainingQuantity { get; set; }
        public int EstimatedDailyCapacity { get; set; }
        public int EstimatedDaysToComplete { get; set; }
        public DateOnly EstimatedFinishDate { get; set; }
    }

    public class ProductionWorkerProgressChartViewDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public int TotalOutput { get; set; }
        public decimal ProgressPercent { get; set; }
    }

    public class WorkerProductivityScoreViewDTO
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int ProductionId { get; set; }
        public int TotalOutput { get; set; }
        public int IssueCount { get; set; }
        public decimal ProductivityScore { get; set; }
    }








}
