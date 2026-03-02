using GPMS.APPLICATION.Enum;
using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class RegisterDTO
    {
        public User User { get; set; } = null!;

        public RegisterStatus Status { get; set; } = RegisterStatus.Failed;

        public List<string> Errors { get; set; } = new List<string>();

    }
}
