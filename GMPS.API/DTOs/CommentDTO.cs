using GPMS.DOMAIN.Constants;
using System.ComponentModel.DataAnnotations;

namespace GMPS.API.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int ToOrderId { get; set; }
        public string Content { get; set; }
        public DateTime SendDateTime { get; set; }
    }
}
