using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class PartPaymentCompletionDTO
    {   
        public int PartId { get; set; }
        public int AffectedLogs { get; set; }
        // Ngày thanh toán
        public DateTime PaidAt { get; set; }
    }

    public class CompletePartPaymentDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> WorkLogIds { get; set; } = new();
    }

}
