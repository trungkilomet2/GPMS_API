using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class RejectCancelLeaveRequestDTO
    {
        [Required(ErrorMessage = "Reject reason is required.")]
        [StringLength(100, ErrorMessage = "Reject reason must not exceed 100 characters.")]
        public string RejectCancelContent { get; set; } = null!;
    }
}
