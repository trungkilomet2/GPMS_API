using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.DTOs
{
    public class AssignWorkerViewDTO
    {
        public User Workers;
        public IEnumerable<WorkerSkill> Skill_Of_Worker { get; set; }
        public IEnumerable<LeaveRequest?> LeaveRequest { get; set; }

    }




}
