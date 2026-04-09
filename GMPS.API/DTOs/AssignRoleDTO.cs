using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "Phải có ít nhất một vai trò")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một vai trò")]
        public List<int> RoleIds { get; set; } = new();
    }
}
