using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateLeaveRequestDTO
    {
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(155, ErrorMessage = "Content must not exceed 155 characters.")]
        public string Content { get; set; } = null!;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
