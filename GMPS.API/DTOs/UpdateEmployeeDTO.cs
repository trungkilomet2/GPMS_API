using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateEmployeeDTO
    {
        [Required(ErrorMessage = "Yêu cầu nhập đầy đủ tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Yêu cầu nhập người quản lý")]
        [Range(1, int.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
        public int ManagerId { get; set; }

        [Required(ErrorMessage = "Yêu cầu chọn vai trò")]
        [MinLength(1, ErrorMessage = "Chọn ít nhất 1 vai trò")]
        public List<int>? RoleIds { get; set; }
    }
}
