using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreateWorkerRoleDTO
    {
        [Required(ErrorMessage ="Role Name is required")]
        public string Name { get; set; }
    }
}
