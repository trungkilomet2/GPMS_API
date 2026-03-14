using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "At least one role must be provided")]
        [MinLength(1, ErrorMessage = "At least one role must be provided")]
        public List<int> RoleIds { get; set; } = new();
    }
}
