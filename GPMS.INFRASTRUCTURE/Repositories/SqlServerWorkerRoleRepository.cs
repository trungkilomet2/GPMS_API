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
    public class SqlServerWorkerRoleRepository : IBaseRepositories<WorkerRole>, IBaseWorkerRoleRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRoleRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<WorkerRole> Create(WorkerRole entity)
        {
            var roleEntity = _mapper.Map<WORKER_ROLE>(entity);

            await _context.WORKER_ROLE.AddAsync(roleEntity);
            await _context.SaveChangesAsync();
            return _mapper.Map<WorkerRole>(roleEntity);
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<WorkerRole> FindRoleByName(string roleName)
        {
            var data = await _context.WORKER_ROLE.FirstOrDefaultAsync(r => r.NAME.ToLower() == roleName.ToLower());
            return _mapper.Map<WorkerRole>(data);
        }

        public async Task<IEnumerable<WorkerRole>> GetAll(object? obj)
        {
            var data = await _context.WORKER_ROLE.ToListAsync();
            return _mapper.Map<List<WorkerRole>>(data);
        }

        public async Task<WorkerRole> GetById(object id)
        {
            var data = await _context.WORKER_ROLE.FindAsync(id);
            return _mapper.Map<WorkerRole>(data);
        }

        public Task<WorkerRole> Update(WorkerRole entity)
        {
            throw new NotImplementedException();
        }
    }
}
