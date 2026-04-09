using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateLeaveRequestDTO
    {
        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        [StringLength(155, ErrorMessage = "Nội dung không được vượt quá 155 ký tự.")]
        public string Content { get; set; } = null!;

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
