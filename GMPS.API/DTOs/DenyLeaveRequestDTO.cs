using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class DenyLeaveRequestDTO
    {
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Lý do từ chối không được vượt quá 100 ký tự.")]
        public string DenyContent { get; set; } = null!;
    }
}