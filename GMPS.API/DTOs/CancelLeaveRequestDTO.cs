using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CancelLeaveRequestDTO
    {
        [Required(ErrorMessage = "Cancel reason is required.")]
        [StringLength(100, ErrorMessage = "Cancel reason must not exceed 100 characters.")]
        public string CancelContent { get; set; } = null!;
    }
}
