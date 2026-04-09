using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateWorkerRoleDTO
    {
        [Required(ErrorMessage ="Yêu cầu nhập tên chuyên môn")]
        [StringLength(50, ErrorMessage = "Tên chuyên môn không được vượt quá 50 ký tự")]
        public string Name { get; set; }
    }
}
