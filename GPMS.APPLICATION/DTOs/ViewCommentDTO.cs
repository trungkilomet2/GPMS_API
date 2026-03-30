using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class ViewCommentDTO
    {
        public Comment Comment { get; set; }
        public string FromUserName { get; set; }
    }
}
