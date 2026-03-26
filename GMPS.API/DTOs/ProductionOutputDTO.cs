namespace GMPS.API.DTOs
{
    public class ProductionOutputDTO
    {
        public class ProductionWorkerOutputDTO
        {
            public int WorkerId { get; set; }
            public string WorkerName { get; set; } = string.Empty;
            public int ProductionId { get; set; }
            public int CuttingOutput { get; set; }
            public int SewingOutput { get; set; }
            public int TotalOutput { get; set; }
            public int IssueCount { get; set; }
        }
        public class WorkerProductivityHistoryDTO
        {
            public int WorkerId { get; set; }
            public string WorkerName { get; set; } = string.Empty;
            public int ProductionId { get; set; }
            public string SourceType { get; set; } = string.Empty;
            public int SourceId { get; set; }
            public int Quantity { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public string? Note { get; set; }
        }
        public class ProductionOutputSummaryDTO
        {
            public int ProductionId { get; set; }
            public int TotalCuttingOutput { get; set; }
            public int TotalSewingOutput { get; set; }
            public int TotalIssueCount { get; set; }
            public int TotalOutput { get; set; }
        }
        public class WorkerAssignedPlanDTO
        {
            public int ProductionId { get; set; }
            public int OrderId { get; set; }
            public string OrderName { get; set; } = string.Empty;
            public int StatusId { get; set; }
            public IEnumerable<string> PartNames { get; set; } = new List<string>();
        }

    }
}
