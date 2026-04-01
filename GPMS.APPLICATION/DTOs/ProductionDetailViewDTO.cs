using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class ProductionDetailViewDTO
    {
        public Production Production { get; set; } = new();
        public User? ProjectManager { get; set; }
        public Order? Order { get; set; }
        public string ProductionStatusName { get; set; }
    }
}
