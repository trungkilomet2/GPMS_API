using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateWorkerRoleDTO
    {
        [Required(ErrorMessage ="Role Name is required")]
        [StringLength(50, ErrorMessage = "tên kỹ năng không được vượt quá 50")]
        public string Name { get; set; }
    }
}
