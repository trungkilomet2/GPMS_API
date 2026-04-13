using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ApproveWorkLogRequestDTO
    {
        [Range(0, int.MaxValue)]
        public int ApprovedQuantity { get; set; }
    }

    public class UpdateIssueStatusRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int StatusId { get; set; }
    }

    public class ConfirmUnfixableIssueRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int ConfirmedQuantity { get; set; }
    }

    public class CreateDeliveryItemDTO
    {
        [Range(1, int.MaxValue)]
        public int OrderSizeId { get; set; }

        [Range(1, int.MaxValue)]
        public int DeliverQuantity { get; set; }

        [Range(1, int.MaxValue)]
        public int DeliverStatusId { get; set; }
    }

    public class CreateDeliveryBatchRequestDTO
    {
        [Required]
        [MinLength(1)]
        public IEnumerable<CreateDeliveryItemDTO> Deliveries { get; set; } = new List<CreateDeliveryItemDTO>();
    }
}
