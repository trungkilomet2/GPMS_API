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

        [Required(ErrorMessage = "Yêu cầu nhập nội dung")]
        [StringLength(500, ErrorMessage = "Nội dung không quá 500 ký tự")]
        public string Content { get; set; }

    }
}
