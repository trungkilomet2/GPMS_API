using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ApproveProductionDTO
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }

    public class RejectProductionRequestDTO
    {

        [Required]
        [StringLength(150)]
        public string Reason { get; set; } = string.Empty;
    }
}