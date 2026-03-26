using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ProductionIssueListItemDTO
    {   

        public int IssueId { get; set; }
        public string TypeIssue { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ProductionIssueSummaryDTO
    {
        public string TypeIssue { get; set; }
        public int TotalIssues { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime? LastIssueAt { get; set; }
    }

    public class CreatePartIssueDTO
    {
        [Range(1, int.MaxValue)]
        public int CreatedBy { get; set; }

        [Range(1, 4)]
        public int Priority { get; set; } = 2;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string TypeIssue { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [FromForm]
        public IFormFile? Image { get; set; }
    }
}