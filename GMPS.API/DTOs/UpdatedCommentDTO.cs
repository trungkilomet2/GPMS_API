using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdatedCommentDTO
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(500, ErrorMessage = "Content cannot exceed 500 characters")]
        public string Content { get; set; }

    }
}
