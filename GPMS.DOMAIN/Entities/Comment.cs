using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int fromUserId { get; set; }
        public int toOrderId { get; set; }
        public string Content { get; set; }
        public DateTime SendDateTime { get; set; }

    }
}
