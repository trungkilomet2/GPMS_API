using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.DOMAIN.Entities
{
    public class User
    {
       public int Id { get; set; }
       public string UserName { get; set; }
       public string PasswordHash { get; set; }
       public string FullName { get; set; }
       public string PhoneNumber { get; set; }
       public string AvartarUrl { get; set; }
       public string Location { get; set; }
       public string Email { get; set; }
       public int StatusId { get; set; }

        public List<Role> Roles { get; set; } = new();

        public List<WorkerSkill> WorkerSkills { get; set; } = new();

        public UserStatus? Status { get; set; }

    }
}
