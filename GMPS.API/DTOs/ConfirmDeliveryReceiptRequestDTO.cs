using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class ConfirmDeliveryReceiptRequestDTO
    {
        [Required]
        [RegularExpression("^(Yes|No)$", ErrorMessage = "Giá trị xác nhận phải là 'Yes' hoặc 'No'.")]
        public string ConfirmationText { get; set; } = string.Empty;
    }
}
