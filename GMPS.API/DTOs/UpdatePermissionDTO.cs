using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdatePermissionDTO
    {
        [Required]
        public List<int> RoleIds { get; set; } = new();
    }
}
