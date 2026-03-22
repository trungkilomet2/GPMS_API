using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class FromDateToEndDate
    {
        public int UserId { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;

    }
}
