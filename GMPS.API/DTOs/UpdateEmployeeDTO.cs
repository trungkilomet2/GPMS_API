using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdateEmployeeDTO
    {
        [Required(ErrorMessage = "FullName is required")]
        [StringLength(50, ErrorMessage = "FullName cannot exceed 50 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "StatusId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "StatusId must be greater than 0")]
        public int ManagerId { get; set; }
        public int StatusId { get; set; }

        [Required(ErrorMessage = "At least one role must be assigned")]
        [MinLength(1, ErrorMessage = "At least one role must be provided")]
        public List<int>? RoleIds { get; set; }
    }
}
