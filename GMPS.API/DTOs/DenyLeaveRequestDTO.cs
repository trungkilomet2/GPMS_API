using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class DenyLeaveRequestDTO
    {
        [Required(ErrorMessage = "Deny reason is required.")]
        [StringLength(100, ErrorMessage = "Deny reason must not exceed 100 characters.")]
        public string DenyContent { get; set; } = null!;
    }
}