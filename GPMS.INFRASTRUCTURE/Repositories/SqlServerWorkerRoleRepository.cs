using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPMS.INFRASTRUCTURE.Repositories
{
    public class SqlServerWorkerRoleRepository : IBaseRepositories<WorkerSkill>, IBaseWorkerRoleRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRoleRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<WorkerSkill> Create(WorkerSkill entity)
        {
            var roleEntity = _mapper.Map<WORKER_SKILL>(entity);

            await _context.WORKER_SKILL.AddAsync(roleEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<WorkerSkill>(roleEntity);
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkerSkill> FindRoleByName(string roleName)
        {
            var data = await _context.WORKER_SKILL.FirstOrDefaultAsync(r => r.NAME.ToLower() == roleName.ToLower());
            return _mapper.Map<WorkerSkill>(data);
        }

        public async Task<IEnumerable<WorkerSkill>> GetAll(object? obj)
        {
            var data = await _context.WORKER_SKILL.ToListAsync();
            return _mapper.Map<List<WorkerSkill>>(data);
        }

        public async Task<WorkerSkill> GetById(object id)
        {
            var data = await _context.WORKER_SKILL.FindAsync(id);
            return _mapper.Map<WorkerSkill>(data);
        }

        public Task<WorkerSkill> Update(WorkerSkill entity)
        {
            throw new NotImplementedException();
        }
    }
}
