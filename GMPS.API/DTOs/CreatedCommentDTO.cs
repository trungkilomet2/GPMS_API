using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CreatedCommentDTO
    {
        [Required(ErrorMessage = "FromUserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "FromUserId must be greater than 0")]
        public int FromUserId { get; set; }

        [Required(ErrorMessage = "ToOrderId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ToOrderId must be greater than 0")]
        public int ToOrderId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(200, ErrorMessage = "Content cannot exceed 200 characters")]
        public string Content { get; set; }

        [Required(ErrorMessage = "SendDateTime is required")]
        public DateTime SendDateTime { get; set; }
    }
}
