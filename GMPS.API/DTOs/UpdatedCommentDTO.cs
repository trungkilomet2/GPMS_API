using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class UpdatedCommentDTO
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(500, ErrorMessage = "Content cannot exceed 500 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Content cannot contain special characters")]
        public string Content { get; set; }

        [Required(ErrorMessage = "SendDateTime is required")]
        public DateTime SendDateTime { get; set; }
    }
}
