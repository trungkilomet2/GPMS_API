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
    public class SqlServerRoleRepository : IBaseRepositories<Role>
    {
        private readonly GPMS_SYSTEMContext _context;
        private readonly IMapper _mapper;

        public SqlServerRoleRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        public Task<Role> Create(Role entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(object id)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<Role>> GetAll(object? obj)
        {
            List<ICollection<ROLE>> userRoles = new List<ICollection<ROLE>>();
            
          

            if (obj is User user)
            {
                userRoles = await _context.USER.Include(u => u.ROLE).Where(u=> u.USER_ID == user.Id).Select(u => u.ROLE).ToListAsync();

            }

            return _mapper.Map<IEnumerable<Role>>(userRoles.SelectMany(ur => ur).ToList()); 
        }

        // Get Role By User Id
        public Task<Role> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<Role> Update(Role entity)
        {
            throw new NotImplementedException();
        }
    }
}
