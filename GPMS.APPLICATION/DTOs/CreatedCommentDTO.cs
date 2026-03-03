using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class CreatedCommentDTO
    {
        public int commentId { get; set; }
        public int fromUserId { get; set; }
        public int toOrderId { get; set; }
        public string Content { get; set; }
        public DateTime SendDateTime { get; set; }
    }
}
