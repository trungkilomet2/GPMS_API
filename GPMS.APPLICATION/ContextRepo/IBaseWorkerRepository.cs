using GPMS.DOMAIN.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.APPLICATION.ContextRepo
{
    public interface IBaseWorkerRepository
    {
        public Task<IEnumerable<User>> GetAll(object? obj);
        public Task<User> GetWorkerById(int id);
        public  Task<User> Create(User entity);
        public Task<User> Update(User entity);
        
        // Insert By TrungNT
        public Task<IEnumerable<WorkerSkill>> GetWorkerSkillByUserId(int userId);    

    }
}
