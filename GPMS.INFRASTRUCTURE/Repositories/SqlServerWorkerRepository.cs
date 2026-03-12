using AutoMapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.DOMAIN.Constants;
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
    public class SqlServerWorkerRepository : IBaseRepositories<User>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<User> Create(User entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<User>> GetAll(object? obj)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).ToListAsync();
            return _mapper.Map<List<User>>(users);
        }

        public async Task<User> GetById(object id)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.USER_ID == (int)id && u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).FirstOrDefaultAsync();
            return _mapper.Map<User>(users);
        }

        public Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
