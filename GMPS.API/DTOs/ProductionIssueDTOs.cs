using GPMS.DOMAIN.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ProductionIssueListItemDTO
    {   

        public int IssueId { get; set; }
        public int PartOrderSizeId { get; set; }
        public string? PartName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; }
        public int Quantity { get; set; }
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int StatusId { get; set; }

        public string Status => StatusId switch
        {
            IssueStatus_Constrants.ToDo_ID => IssueStatus_Constrants.ToDo,
            IssueStatus_Constrants.Processing_ID => IssueStatus_Constrants.Processing,
            IssueStatus_Constrants.Fixed_ID => IssueStatus_Constrants.Fixed,
            IssueStatus_Constrants.Error_ID => IssueStatus_Constrants.Error,
            _ => "Không Xác Định"
        };

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

        [Range(1, int.MaxValue)]
        public int? AssignedTo { get; set; }

        [Range(1, 4)]
        public int Priority { get; set; } = 2;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        
        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime? OccurredAt { get; set; }

        [FromForm]
        public IFormFile? Image { get; set; }
    }
}