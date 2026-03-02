using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class LoginDTO
    {
        public User User { get; set; }

        public IEnumerable<Role> UserRole { get; set; }

    }
}
