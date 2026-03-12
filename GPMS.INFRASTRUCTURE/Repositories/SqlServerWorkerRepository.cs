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
    public class SqlServerWorkerRepository : IBaseWorkerRepositories
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerWorkerRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> GetEmployeeById(int id)
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.USER_ID == id && u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).FirstOrDefaultAsync();
            return _mapper.Map<User>(users);
        }

        public async Task<IEnumerable<User>> GetEmployees()
        {
            var users = await _context.USER
                              .Include(u => u.ROLE)
                              .Include(u => u.WR)
                              .Include(u => u.US)
                              .Where(u => u.ROLE.Any(r => r.NAME == Roles_Constants.PM || r.NAME == Roles_Constants.Team_Leader ||
                              r.NAME == Roles_Constants.Worker || r.NAME == Roles_Constants.KCS)).ToListAsync();
            return _mapper.Map<List<User>>(users);
        }
    }
}
