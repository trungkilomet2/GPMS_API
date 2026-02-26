using AutoMapper;
using GPMS.APPLICATION.Repositories;
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
    public class SqlServerUserRepository : IBaseRepository<User>
    {
        private readonly GPMS_SYSTEMContext context;
        private readonly IMapper mapper;

        public SqlServerUserRepository(GPMS_SYSTEMContext context, IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> Create(User entity)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var data = await context.USER.ToListAsync();

            return mapper.Map<IEnumerable<GPMS.DOMAIN.Entities.User>>(data);
        }

        public async Task<User> GetById(object id)
        {
            throw new NotImplementedException();
        }

        public async Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
